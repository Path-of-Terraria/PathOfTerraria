using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.EnergyShield;
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
		string text = $"Life: {plr.statLife}/{plr.statLifeMax2}";
		int overheal = plr.GetModPlayer<OverhealthPlayer>().Overhealth;
		EnergyShieldPlayer energyShield = plr.GetModPlayer<EnergyShieldPlayer>();

		if (overheal > 0)
		{
			text += $" ([c/FF9BF3:+{overheal}])";
		}

		if (energyShield.MaximumEnergyShield > 0)
		{
			text += $"\nEnergy Shield: {energyShield.CurrentEnergyShieldRounded}/{energyShield.MaximumEnergyShield}";
		}
		
		Main.instance.MouseTextHackZoom(text);
	}

	public void Unload()
	{
	}
}
