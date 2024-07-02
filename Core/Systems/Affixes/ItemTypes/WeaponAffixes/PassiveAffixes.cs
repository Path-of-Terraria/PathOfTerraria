namespace PathOfTerraria.Core.Systems.Affixes.ItemTypes.WeaponAffixes;

internal class IncreasedAttackSpeedAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.Attackspeed += (Value * 5 + Value * item.ItemLevel / 10f) / 100f;
	}
}

internal class AddedAttackSpeedAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.Attackspeed.Base += (Value * 5 + Value * item.ItemLevel / 10f) / 100f;
	}
}

internal class AddedDamageAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.Damage.Base += (Value * 5 + Value * item.ItemLevel / 20) / 100f;
	}
}

internal class IncreasedDamageAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.Damage += (Value * 5 + Value * item.ItemLevel / 20) / 100f;
	}
}

internal class ChargedItemAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.Damage *= 1 + (Value * 5 + Value * item.ItemLevel / 20) / 100f;
	}
}