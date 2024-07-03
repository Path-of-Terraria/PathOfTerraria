namespace PathOfTerraria.Core.Systems.Affixes.ItemTypes;

internal class DefenseItemAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.Defense.Base += Value;
	}
}

internal class EnduranceItemAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.DamageReduction.Base += Value;
	}
}

internal class ResistanceHelmetAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.DebuffResistance *= Value / 100;
	}
}

internal class BuffBoostHelmetAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.BuffBonus *= Value / 100;
	}
}

internal class ThornyArmorAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.ReflectedDamageModifier += Value;
	}
}