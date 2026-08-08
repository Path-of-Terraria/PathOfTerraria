using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class DangerMiningPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.DistanceSQ(player.Center) < PoTMod.NearbyDistanceSq)
			{
				player.pickSpeed *= MathHelper.Clamp(1f - Value / 100f, 0.1f, 1f);
				break;
			}
		}
	}
}
