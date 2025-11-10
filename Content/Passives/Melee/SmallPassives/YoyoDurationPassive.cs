using Humanizer;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Utilities;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives;

internal class YoyoDurationPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<YoyoStatsPlayer>().YoyoLifeTime *= (1 + ((Value/100) * Level));
	}
}