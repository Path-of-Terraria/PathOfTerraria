using System.Text.RegularExpressions;

namespace PathOfTerraria.Common.Utilities;

public static class LocalizationUtils
{
	public static bool IsCjkPunctuation(char a)
	{
		return Regex.IsMatch(a.ToString(), @"\p{IsCJKSymbolsandPunctuation}|\p{IsHalfwidthandFullwidthForms}");
	}

	public static bool IsCjkUnifiedIdeographs(char a)
	{
		return Regex.IsMatch(a.ToString(), @"\p{IsCJKUnifiedIdeographs}");
	}

	public static bool IsRightCloseCjkPunctuation(char a)
	{
		return a is '（' or '【' or '《' or '｛' or '｢' or '［' or '｟' or '“';
	}

	public static bool IsCjkCharacter(char a)
	{
		return IsCjkUnifiedIdeographs(a) || IsCjkPunctuation(a);
	}
}