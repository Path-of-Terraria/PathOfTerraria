using Microsoft.Build.Framework;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using PathOfTerraria.Core.Systems.Affixes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.IO;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PathOfTerraria.Core;

internal class Line(string _prefix, string _tagString, string _val)
{
	protected string prefix = _prefix;
	protected string tagString = _tagString;
	protected string val = _val;

	public int PrefixSize => prefix.Length;
	public string Text => prefix.Length == 0 ? val.ToString() : prefix + ": " + val.ToString();

	public virtual void AddTo(TagCompound tag)
	{
		tag[tagString] = val;
	}

	public virtual void LoadString(string _prefix, string _tagString, string _val)
	{
		prefix = _prefix;
		tagString = _tagString;
		val = _prefix.Length == 0 ? _val : _val[(_prefix.Length + 2)..];
	}
}
internal class Line<T>(string _prefix, string _tagString, object _val) : Line(_prefix, _tagString, _val is null ? "" : _val.ToString())
{
	public virtual void AddTo(TagCompound tag)
	{
		if (typeof(T) == typeof(int))
		{
			tag[tagString] = int.Parse(val);
		}
		else if (typeof(T) == typeof(float))
		{
			tag[tagString] = float.Parse(val);
		}
		else
		{
			tag[tagString] = val;
		}
	}
}
internal class EnumLine<T>(string _prefix, string _tagString, object _val) : Line(_prefix, _tagString, _val is null ? "" : Enum.GetName(typeof(T), _val)) where T : struct
{
	public override void AddTo(TagCompound tag)
	{
		if (Enum.TryParse(val.ToString(), true, out T nVal))
		{
			tag[tagString] = Convert.ChangeType(nVal, typeof(int));
		}
	}
}
internal class StringTagRelationAffix(string name, float strength, float min, float max)
	: Line<string>("", "", name.Split(['.', '+']).Last() + ": " + strength + " | " + min + " - " + max)
{
	public override void AddTo(TagCompound tag)
	{
		foreach (ItemAffix a in AffixHandler.GetAffixes())
		{
			if (a.GetType().FullName.ToLower().Contains(name.ToLower()))
			{
				tag["type"] = a.GetType().FullName;
			}
		}

		tag["value"] = strength;
		tag["minValue"] = min;
		tag["maxValue"] = max;
	}

	private static Regex regex = new("(^.+): (.+) \\| (.+) \\- (.+)");
	public static StringTagRelationAffix FromString(string s)
	{
		string[] strings = regex.Split(s);
		return new(strings[1], float.Parse(strings[2]), float.Parse(strings[3]), float.Parse(strings[4]));
	}
}

internal class StringTagRelationAffixList(int _implicits)
{
	public List<StringTagRelationAffix> List = [];
	public int Implicits = _implicits;

	public void Add(string v1, float v2, float v3, float v4)
	{
		List.Add(new(v1, v2, v3, v4));
	}

	public void AddTo(TagCompound tag)
	{
		tag["implicits"] = Implicits;

		List<TagCompound> tags = [];

		foreach(StringTagRelationAffix rel in List)
		{
			TagCompound nTag = new();
			rel.AddTo(nTag);
			tags.Add(nTag);
		}

		tag["affixes"] = tags;
	}

	public void AddTo(StringBuilder sb)
	{
		if (Implicits > 0)
		{
			sb.AppendLine("[Implicits]");
		}

		int implicitsLeft = Implicits;

		foreach (StringTagRelationAffix affix in List)
		{
			if (implicitsLeft == 0)
			{
				sb.AppendLine("[Affixes]");
			}
			sb.AppendLine(affix.Text);
			implicitsLeft--;
		}
	}

	public static StringTagRelationAffixList FromStrings(string[] strings)
	{
		StringTagRelationAffixList ret = new(0);

		strings = strings[1..]; // remove [Implicits] / [Affixes]

		int implicits = 0;

		foreach (string s in strings)
		{
			if (s == "[Affixes]")
			{
				ret.Implicits = implicits == 0 ? 0 : implicits;
			}
			else
			{
				ret.List.Add(StringTagRelationAffix.FromString(s));
			}
			implicits++;
		}

		return ret;
	}
}

internal class StringTagRelation
{
	static readonly Tuple<string, string, Type>[] lineTypes = [
		new("Item Type", "type", typeof(EnumLine<ItemType>)),
		new("Rarity", "rarity", typeof(EnumLine<Rarity>)),
		new("", "name", typeof(Line<string>)),
		new("Power", "power", typeof(Line<int>)),
		new("Influence", "influence", typeof(EnumLine<Influence>)),
	];

	readonly Line[] lines = new Line[5];
	StringTagRelationAffixList affixes;
	private void SetLines(TagCompound tag)
	{
		for (int i = 0; i < lineTypes.Length; i++)
		{
			lines[i] = (Line)Activator.CreateInstance(lineTypes[i].Item3, [lineTypes[i].Item1, lineTypes[i].Item2, tag[lineTypes[i].Item2]]);
		}
	}

	private void SetLine(int line, string text)
	{
		if (line >= lineTypes.Length)
		{
			return;
		}

		lines[line] = (Line)Activator.CreateInstance(lineTypes[line].Item3, ["", "", null]);
		lines[line].LoadString(lineTypes[line].Item1, lineTypes[line].Item2, text);
	}

	private TagCompound ToTagCompound()
	{
		TagCompound tag = new();
		
		foreach(Line line in lines)
		{
			if (line is Line<int> iLine)
			{
				iLine.AddTo(tag);
			}
			else if (line is Line<float> fLine)
			{
				fLine.AddTo(tag);
			}
			else
			{
				line.AddTo(tag);
			}
		}

		affixes.AddTo(tag);

		return tag;
	}

	public static TagCompound FromString(string s, TagCompound fallback)
	{
		try
		{
			StringTagRelation relation = new StringTagRelation();
			StringReader sr = new(s);

			for (int i = 0; i < lineTypes.Length; i++)
			{
				relation.SetLine(i, sr.ReadLine());
			}

			List<string> lines = [];
			string next = sr.ReadLine();
			while (next is not null)
			{
				lines.Add(next);
				next = sr.ReadLine();
			}

			relation.affixes = StringTagRelationAffixList.FromStrings(lines.ToArray());
			return relation.ToTagCompound();
		}
		catch
		{
			return fallback;
		}
	}

	public string AsString()
	{
		StringBuilder sb = new StringBuilder();

		foreach (Line line in lines)
		{
			sb.AppendLine(line.Text);
		}

		affixes.AddTo(sb);

		return sb.ToString();
	}

	public static string FromTag(TagCompound tag)
	{
		StringTagRelation relation = new StringTagRelation();
		relation.SetLines(tag);

		relation.affixes = new(tag.GetInt("implicits"));

		IList<TagCompound> affixTags = tag.GetList<TagCompound>("affixes");

		foreach (TagCompound newTag in affixTags)
		{
			relation.affixes.Add(
				newTag.GetString("type"),
				newTag.GetFloat("value"),
				newTag.GetFloat("maxValue"),
				newTag.GetFloat("minValue")
			);
		}

		return relation.AsString();
	}
}

/*

		tag["type"] = (int)ItemType;
		tag["rarity"] = (int)Rarity;
		tag["influence"] = (int)Influence;

		tag["implicits"] = _implicits;

		tag["name"] = _name;
		tag["power"] = InternalItemLevel;

		List<TagCompound> affixTags = [];
		foreach (ItemAffix affix in Affixes)
		{
			var newTag = new TagCompound();
			affix.Save(newTag);
			affixTags.Add(newTag);
		}

		tag["affixes"] = affixTags;
 */