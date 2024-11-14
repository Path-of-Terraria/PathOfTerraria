namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal class IncreasedAttackSpeedAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.Attackspeed += Value;
	}

	public override void ApplyTooltip(Player player, Item item, AffixTooltipsHandler handler)
	{
		handler.AddOrModify(GetType(), item, Value, this.GetLocalization("Description"), IsCorruptedAffix, null);
	}
}

internal class AddedAttackSpeedAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.Attackspeed.Base += Value;
	}
}

internal class AddedDamageAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.Damage.Base += Value;
	}
}

internal class IncreasedDamageAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.Damage += Value / 500;
	}

	public override void ApplyTooltip(Player player, Item item, AffixTooltipsHandler handler)
	{
		handler.AddOrModify(GetType(), item, Value / 5, this.GetLocalization("Description"), IsCorruptedAffix, null);
	}
}