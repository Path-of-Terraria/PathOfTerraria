using Terraria.ID;

namespace PathOfTerraria.Common.Systems.VanillaModifications.BossItemRemovals;

internal class DisableEoLSpawn : ModSystem
{
	public override void Load()
	{
		On_NPC.DoDeathEvents += HijackLacewingDeathEffect;
	}

	private void HijackLacewingDeathEffect(On_NPC.orig_DoDeathEvents orig, NPC self, Player closestPlayer)
	{
		if (self.type == NPCID.EmpressButterfly)
		{
			return;
		}

		orig(self, closestPlayer);
	}
}
