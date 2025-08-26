using PathOfTerraria.Common.Systems.ModPlayers;

namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal class StrengthItemAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<AttributesPlayer>().Strength += (int)Math.Round(Value);
	}

	public override void ApplyTooltip(Player player, Item item, AffixTooltips handler)
	{
		handler.AddOrModify(GetType(), new AffixTooltipLine
		{
			Text = this.GetLocalization("Description"),
			Value = (int)Math.Round(Value),
			Corrupt = IsCorruptedAffix,
		});
	}
}

internal class DexterityItemAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<AttributesPlayer>().Dexterity += (int)Math.Round(Value);
	}

	public override void ApplyTooltip(Player player, Item item, AffixTooltips handler)
	{
		handler.AddOrModify(GetType(), new AffixTooltipLine
		{
			Text = this.GetLocalization("Description"),
			Value = (int)Math.Round(Value),
			Corrupt = IsCorruptedAffix,
		});
	}
}

internal class IntelligenceItemAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<AttributesPlayer>().Intelligence += (int)Math.Round(Value);
	}

	public override void ApplyTooltip(Player player, Item item, AffixTooltips handler)
	{
		handler.AddOrModify(GetType(), new AffixTooltipLine
		{
			Text = this.GetLocalization("Description"),
			Value = (int)Math.Round(Value),
			Corrupt = IsCorruptedAffix,
		});
	}
}
