using Humanizer;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Utilities;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives;

internal class YoyoDurationPassive : Passive
{
	public const float DurationIncrease = 1f;

	public override string DisplayTooltip => Language.GetTextValue($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").FormatWith(MathUtils.Percent(DurationIncrease));

	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<YoyoStatsPlayer>().YoyoLifeTime *= (1 + (DurationIncrease * Level));
	}
}