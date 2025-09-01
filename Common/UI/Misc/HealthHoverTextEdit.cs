using PathOfTerraria.Common.Systems.ModPlayers;
using Terraria.GameContent.UI.ResourceSets;

namespace PathOfTerraria.Common.UI.Misc;

internal class HealthHoverTextEdit : ILoadable
{
	public void Load(Mod mod)
	{
		On_CommonResourceBarMethods.DrawLifeMouseOver += AddText;
	}

	private void AddText(On_CommonResourceBarMethods.orig_DrawLifeMouseOver orig)
	{
		orig();

		Player plr = Main.LocalPlayer;
		string text = $"{plr.statLife}/{plr.statLifeMax2}";
		int overheal = plr.GetModPlayer<OverhealthPlayer>().Overhealth;

		if (overheal > 0)
		{
			text += $" ([c/785B87:+{overheal}])";
		}
		
		Main.instance.MouseTextHackZoom(text);
	}

	public void Unload()
	{
	}
}
