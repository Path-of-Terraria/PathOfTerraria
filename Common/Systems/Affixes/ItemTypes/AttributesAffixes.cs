using PathOfTerraria.Common.Systems.ModPlayers;

namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal abstract class AttributeItemAffix : ItemAffix
{
	protected AttributeItemAffix()
	{
		Round = true;
	}
}

internal class StrengthItemAffix : AttributeItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<AttributesPlayer>().Strength += (int)Math.Round(Value);
	}

	protected override AffixTooltipLine CreateDefaultTooltip(Player player, Item item)
	{
		return base.CreateDefaultTooltip(player, item) with { Value = (int)Math.Round(Value) };
	}
}

internal class DexterityItemAffix : AttributeItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<AttributesPlayer>().Dexterity += (int)Math.Round(Value);
	}

	protected override AffixTooltipLine CreateDefaultTooltip(Player player, Item item)
	{
		return base.CreateDefaultTooltip(player, item) with { Value = (int)Math.Round(Value) };
	}
}

internal class IntelligenceItemAffix : AttributeItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<AttributesPlayer>().Intelligence += (int)Math.Round(Value);
	}

	protected override AffixTooltipLine CreateDefaultTooltip(Player player, Item item)
	{
		return base.CreateDefaultTooltip(player, item) with { Value = (int)Math.Round(Value) };
	}
}
