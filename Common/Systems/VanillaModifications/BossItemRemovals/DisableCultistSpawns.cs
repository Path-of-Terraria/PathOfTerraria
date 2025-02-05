using Terraria.GameContent.Events;

namespace PathOfTerraria.Common.Systems.VanillaModifications.BossItemRemovals;

internal class DisableCultistSpawns : ModSystem
{
	public override void Load()
	{
		On_CultistRitual.CheckRitual += ForceDisableRitual;
		On_WorldGen.TriggerLunarApocalypse += StopLunarApocalypse;
	}

	private void StopLunarApocalypse(On_WorldGen.orig_TriggerLunarApocalypse orig)
	{
		// TODO: Find more compatible way to stop this entirely
	}

	private bool ForceDisableRitual(On_CultistRitual.orig_CheckRitual orig, int x, int y)
	{
		orig(x, y);
		return false;
	}
}
