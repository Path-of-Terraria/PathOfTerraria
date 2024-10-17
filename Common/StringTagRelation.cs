using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.Affixes;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common;

internal class Line(string _prefix, string _tagString, string _val)
{
	protected string Prefix = _prefix;
	protected string TagString = _tagString;
	protected string Val = _val;

	public int PrefixSize => Prefix.Length;
	public string Text => Prefix.Length == 0 ? Val.ToString() : Prefix + ": " + Val.ToString();

	public virtual void AddTo(TagCompound tag)
	{
		tag[TagString] = Val;
	}

	public virtual void LoadString(string _prefix, string _tagString, string _val)
	{
		Prefix = _prefix;
		TagString = _tagString;
		Val = _prefix.Length == 0 ? _val : _val[(_prefix.Length + 2)..];
	}
}
internal class Line<T>(string _prefix, string _tagString, object _val) : Line(_prefix, _tagString, _val is null ? "" : _val.ToString())
{
	public virtual void AddTo(TagCompound tag)
	{
		if (typeof(T) == typeof(int))
		{
			tag[TagString] = int.Parse(Val);
		}
		else if (typeof(T) == typeof(float))
		{
			tag[TagString] = float.Parse(Val);
		}
		else
		{
			tag[TagString] = Val;
		}
	}
}
internal class EnumLine<T>(string _prefix, string _tagString, object _val) : Line(_prefix, _tagString, _val is null ? "" : Enum.GetName(typeof(T), _val)) where T : struct
{
	public override void AddTo(TagCompound tag)
	{
		if (Enum.TryParse(Val.ToString(), true, out T nVal))
		{
			tag[TagString] = Convert.ChangeType(nVal, typeof(int));
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

	private static readonly Regex regex = new("(^.+): (.+) \\| (.+) \\- (.+)");
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
			TagCompound nTag = [];
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
		new("Rarity", "rarity", typeof(EnumLine<ItemRarity>)),
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
		TagCompound tag = [];
		
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