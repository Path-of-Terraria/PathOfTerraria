using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Affixes.Maps;

public class MapMobChillChanceAffix : MapAffix
{
	public override void OnHitPlayer(NPC npc, Player player, Player.HurtInfo info)
	{
		if (Main.rand.NextFloat() < Value / 100f)
		{
			player.AddBuff(BuffID.Chilled, 4 * 60);
		}
	}
}
