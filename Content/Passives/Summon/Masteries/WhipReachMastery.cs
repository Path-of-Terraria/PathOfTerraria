using Humanizer;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Utilities;
using Terraria.Localization;

namespace PathOfTerraria.Content.Passives.Summon.Masteries;

internal class WhipReachMastery : Passive
{
	public const float RangeIncrease = 0.5f;
	public const float UseSpeedDecrease = 0.25f;

	public override string DisplayTooltip => Language.GetTextValue($"Mods.PathOfTerraria.Passives.{Name}.Tooltip").FormatWith(MathUtils.Percent(RangeIncrease), MathUtils.Percent(UseSpeedDecrease));

	public override void BuffPlayer(Player player)
	{
		player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) -= UseSpeedDecrease;
		player.GetModPlayer<WhipReachPlayer>().WhipReach += RangeIncrease;
	}
}