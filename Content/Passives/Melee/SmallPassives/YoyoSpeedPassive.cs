using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class YoyoSpeedPassive : Passive
{
	public const float SpeedIncrease = 0.1f;

	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<YoyoStatsPlayer>().YoyoSpeed *= (1 + (SpeedIncrease * Level));
	}
}