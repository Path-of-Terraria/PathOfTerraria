namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal class IncreasedAttackSpeedAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.Attackspeed += Value / 100;
	}
}

internal class AddedAttackSpeedAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.Attackspeed.Base += Value;
	}
}

internal class AddedDamageAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.Damage.Base += Value;
	}
}

internal class IncreasedDamageAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.Damage += Value / 100;
	}
}