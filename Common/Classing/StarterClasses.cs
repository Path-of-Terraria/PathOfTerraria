using Terraria.Localization;

namespace PathOfTerraria.Common.Classing;

public enum StarterClasses : sbyte
{
	None = -1,
	Melee = 0,
	Ranged,
	Magic,
	Summon
}

public static class StarterClassExtensions
{
	public static LocalizedText Localize(this StarterClasses classes)
	{
		return Language.GetText("Mods.PathOfTerraria.UI.ClassPages.StarterClasses." + classes.ToString());
	}

	public static string GetNouns(this StarterClasses classes)
	{
		return Language.GetTextValue("Mods.PathOfTerraria.UI.ClassPages." + classes.ToString() + ".Nouns");
	}
}