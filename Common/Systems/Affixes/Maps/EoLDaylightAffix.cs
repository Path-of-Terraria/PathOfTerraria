using PathOfTerraria.Common.Systems.Affixes;

namespace PathOfTerraria.Common.Systems.Affixes.Maps;

public class EoLDaylightAffix : MapAffix
{
	protected override AffixTooltipLine CreateDefaultTooltip(Player player, Item item)
	{
		return new AffixTooltipLine
		{
			Text = this.GetLocalization("Description"),
			Value = Value,
			Tier = null,
			ValueRollRange = null,
			Corrupt = IsCorruptedAffix,
			Implicit = IsImplicit,
		};
	}
}
