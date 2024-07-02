namespace PathOfTerraria.Core.Systems.Affixes.ItemTypes.ArmorAffixes;

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
		modifier.DebuffResistance *= Value;
	}
}

internal class BuffBoostHelmetAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.BuffBonus *= Value;
	}
}

internal class ThornyArmorAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.ReflectedDamageModifier += Value * 0.1f;
	}
}