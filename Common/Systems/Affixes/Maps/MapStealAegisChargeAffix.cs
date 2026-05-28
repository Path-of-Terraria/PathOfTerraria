using PathOfTerraria.Common.Systems.Charges;

namespace PathOfTerraria.Common.Systems.Affixes.Maps;

public class MapStealAegisChargeAffix : MapAffix
{
	public override void OnHitPlayer(NPC npc, Player player, Player.HurtInfo info)
	{
		player.GetModPlayer<AegisChargePlayer>().RemoveCharge();
	}
}
