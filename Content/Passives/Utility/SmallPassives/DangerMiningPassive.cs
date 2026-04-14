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
				player.pickSpeed *= 1 + Value / 100f;
				break;
			}
		}
	}
}
