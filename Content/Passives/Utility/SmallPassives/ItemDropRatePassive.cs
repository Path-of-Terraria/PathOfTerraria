using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class ItemDropRatePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<ItemDropModifierPlayer>().ItemDropRateMultiplier += Value / 100f;
	}
}
