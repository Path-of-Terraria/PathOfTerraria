using PathOfTerraria.Common.Looting.VirtualBagUI;
using PathOfTerraria.Core.UI;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.UI;

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

	public override void OnEnterWorld()
	{
		if (Main.myPlayer == Player.whoAmI && Class == StarterClasses.None)
		{
			UIManager.TryToggleOrRegister("Temp Class UI", "Vanilla: Mouse Text", new ClassUIState(() => UIManager.TryDisable("Temp Class UI"), Player), 0, InterfaceScaleType.UI);
		}
	}
}
