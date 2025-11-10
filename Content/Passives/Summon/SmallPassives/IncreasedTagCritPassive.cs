using Humanizer;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Utilities;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives;

internal class IncreasedTagCritPassive : Passive
{
	public const float ChanceBonus = 0.1f;

	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.SummonCritChance += ChanceBonus;
	}

	public override string DisplayTooltip => Language.GetTextValue($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").FormatWith(MathUtils.Percent(ChanceBonus));
}