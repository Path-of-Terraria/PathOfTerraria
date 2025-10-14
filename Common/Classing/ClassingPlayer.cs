using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Classing;

internal class ClassingPlayer : ModPlayer
{
	public StarterClasses Class = StarterClasses.None;

	/// <summary>
	/// Gets the class noun, i.e. Warrior or Marksman. Returns "Stranger" if the player has no selected class.
	/// </summary>
	public static string GetClassNoun(Player player, int index = 0)
	{
		ClassingPlayer plr = player.GetModPlayer<ClassingPlayer>();
		string noun;

		if (plr.Class == StarterClasses.None)
		{
			noun = Language.GetTextValue("Mods.PathOfTerraria.UI.ClassPages.DefaultNouns");
		}
		else
		{
			noun = plr.Class.GetNouns();
		}

		return noun.Split(' ')[index];
	}

	public override void SaveData(TagCompound tag)
	{
		tag.Add("class", (byte)Class);
	}

	public override void LoadData(TagCompound tag)
	{
		Class = (StarterClasses)tag.GetByte("class");
	}
}
