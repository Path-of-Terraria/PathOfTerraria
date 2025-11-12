using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class AddedSummonCritChancePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.SummonCritChance += Value / 100f;
	}
}