using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class MorePickupPotionsPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.MaxHealthPotions += Value;
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.MaxManaPotions += Value;
	}
}
