using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class YoyoRangePassive : Passive
{
	public const float RangeIncrease = 0.1f;

	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<YoyoStatsPlayer>().YoyoRange *= (1 + (RangeIncrease * Level));
	}
}