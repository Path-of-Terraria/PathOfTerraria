using Humanizer;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Utilities;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives;

internal class YoyoRangePassive : Passive
{
	public const float RangeIncrease = 0.1f;

	public override string DisplayTooltip => Language.GetTextValue($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").FormatWith(MathUtils.Percent(RangeIncrease));

	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<YoyoStatsPlayer>().YoyoRange *= (1 + (RangeIncrease * Level));
	}
}