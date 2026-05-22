using PathOfTerraria.Common.Systems.Charges;

namespace PathOfTerraria.Common.Systems.Affixes.Maps;

public class MapStealHasteChargeAffix : MapAffix
{
	public override void OnHitPlayer(NPC npc, Player player, Player.HurtInfo info)
	{
		player.GetModPlayer<HasteChargePlayer>().RemoveCharge();
	}
}
