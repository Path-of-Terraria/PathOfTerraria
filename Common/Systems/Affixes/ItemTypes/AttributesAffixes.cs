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
		player.GetModPlayer<AttributesPlayer>().Strength += (int)Value;
	}
}

internal class DexterityItemAffix : AttributeItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<AttributesPlayer>().Dexterity += (int)Value;
	}
}

internal class IntelligenceItemAffix : AttributeItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<AttributesPlayer>().Intelligence += (int)Value;
	}
}
