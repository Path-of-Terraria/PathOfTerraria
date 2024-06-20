using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PathOfTerraria.Core;

/// <summary>
/// This most likely only works for items; it dosent treat all cases and it sure dosent treat all the cases it treats right.
/// </summary>
internal class StringToTag
{
	public static Tuple<string, object> GetObjectFromString(string objString)
	{
		int begin = objString.IndexOf('"');
		int end = objString.IndexOf('"', begin+1);
		string tag = objString.Substring(begin+1, end - begin - 1);

		if (objString[0..6] == "string")
		{
			string val = objString[(end + 3)..^1];

			return new(tag, val);
		}
		else if (objString[0..3] == "int")
		{
			if (!int.TryParse(objString[(end + 2)..], out int val))
			{
				return null;
			}

			return new(tag, val);
		}
		else if (objString[0..5] == "float")
		{
			if (!float.TryParse(objString[(end + 2)..], out float val))
			{
				return null;
			}

			return new(tag, val);
		}
		else if (objString[0..6] == "object")
		{
			List<string> strings = [];

			int objectDepth = 0;
			bool isInString = false;
			string s = "";
			foreach (char c in objString[(end + 2)..^1])
			{
				if (c == '{')
				{
					objectDepth++;
				}
				else if (c == '}')
				{
					objectDepth--;
				}

				if (c == ',' && objectDepth == 0)
				{
					strings.Add(s);
					s = "";
				}
				else if ((c == '\r' || c == '\n' || c == ' ') && !isInString)
				{
					continue;
				}
				else if (c == '"')
				{
					isInString = !isInString;
					s += c;
				}
				else
				{
					s += c;
				}
			}

			strings.Add(s);

			List<TagCompound> tags = new List<TagCompound>();

			foreach (string tagS in strings)
			{
				TagCompound val = TagFromString(tagS);
				tags.Add(val);
			}

			return new(tag, tags);
		}

		return null;
	}

	public static TagCompound TagFromString(string tagString)
	{
		TagCompound tag = new TagCompound();

		if (tagString.Length == 0 || !(tagString[0] == '{' && tagString.Last() == '}'))
		{
			return tag;
		}

		tagString = tagString[1..];

		int objectDepth = 0;
		bool isInString = false;

		string s = "";
		foreach (char c in tagString)
		{
			if (c == '[')
			{
				objectDepth++;
			}
			else if (c == ']')
			{
				objectDepth--;
			}

			if ((c == ',' || c == '}') && objectDepth == 0)
			{
				Tuple<string, object> t = GetObjectFromString(s);
				tag[t.Item1] = t.Item2;
				s = "";
			}
			else if ((c == '\r' || c == '\n' || c == ' ') && !isInString)
			{
				continue;
			}
			else if (c == '"')
			{
				isInString = !isInString;
				s += c;
			}
			else
			{
				s += c;
			}
		}

		return tag;
	}
}
