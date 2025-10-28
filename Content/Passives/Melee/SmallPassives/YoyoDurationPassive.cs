using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class YoyoDurationPassive : Passive
{
	public const float DurationIncrease = 1f;

	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<YoyoStatsPlayer>().YoyoLifeTime *= (1 + (DurationIncrease * Level));
	}
}