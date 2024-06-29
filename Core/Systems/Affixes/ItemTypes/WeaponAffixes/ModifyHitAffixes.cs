using PathOfTerraria.Content.Buffs;
using Terraria.ID;

namespace PathOfTerraria.Core.Systems.Affixes.ItemTypes.WeaponAffixes;

internal class PiercingItemAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem gear)
	{
		modifier.ArmorPenetration.Base += Value * 5 + gear.ItemLevel / 50;
	}
}

internal class AddedKnockbackItemAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem gear)
	{
		modifier.Knockback += (Value * 10 + gear.ItemLevel / 20) / 100f;
	}
}

internal class IncreasedKnockbackItemAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem gear)
	{
		modifier.Knockback *= 1f + (Value * 10 + gear.ItemLevel / 20) / 100f;
	}
}

internal class BaseKnockbackItemAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem gear)
	{
		modifier.Knockback.Base += Value * gear.ItemLevel / 100;
	}
}

internal class FlatKnockbackItemAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem gear)
	{
		modifier.Knockback.Flat += Value * gear.ItemLevel / 60;
	}
}
	
internal class ChanceToApplyOnFireGearAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem gear)
	{
		modifier.Buffer.Add(BuffID.OnFire, 180, 0.2f);
	}
}
	
internal class ChanceToApplyArmorShredGearAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem gear)
	{
		modifier.Buffer.Add(ModContent.BuffType<ArmorShredDebuff>(), Duration, Value);
	}
}