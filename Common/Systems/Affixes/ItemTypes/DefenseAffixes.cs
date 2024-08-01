namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal class DefenseItemAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.Defense.Base += Value;
	}
}

internal class EnduranceItemAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.DamageReduction.Base += Value;
	}
}

internal class ResistanceHelmetAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.DebuffResistance *= Value / 100;
	}
}

internal class BuffBoostHelmetAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.BuffBonus *= Value / 100;
	}
}

internal class ThornyArmorAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.ReflectedDamageModifier += Value;
	}
}