using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal class NoFallDamageAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		if (player != null)
		{
			player.noFallDmg = true;
		}
	}

	protected override AffixTooltipLine CreateDefaultTooltip(Player player, Item item)
	{
		return base.CreateDefaultTooltip(player, item) with
		{
			TextWhenRemoved = this.GetLocalization("Removed"),
			Value = 1f,
			ValueRollRange = null,
		};
	}
}
