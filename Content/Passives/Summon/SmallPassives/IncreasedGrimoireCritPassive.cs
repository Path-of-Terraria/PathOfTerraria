using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class IncreasedGrimoireCritPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<GrimoirePlayer>().Stats.CriticalStrikeChanceModifier += Value;
	}
}