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

	public override void ApplyTooltip(Player player, Item item, AffixTooltips handler)
	{
		handler.AddOrModify(GetType(), new AffixTooltipLine
		{
			Text = this.GetLocalization("Description"),
			TextWhenRemoved = this.GetLocalization("Removed"),
			Value = 1f,
			Corrupt = IsCorruptedAffix,
		});
	}
}
