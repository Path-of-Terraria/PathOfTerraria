using PathOfTerraria.Content.Buffs;
using Terraria.ID;

namespace PathOfTerraria.Core.Systems.Affixes.ItemTypes;

internal class PiercingItemAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem gear)
	{
		modifier.ArmorPenetration.Base += Value;
	}
}

internal class AddedKnockbackItemAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem gear)
	{
		modifier.Knockback += Value;
	}
}

internal class IncreasedKnockbackItemAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem gear)
	{
		modifier.Knockback *= Value;
	}
}

internal class BaseKnockbackItemAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem gear)
	{
		modifier.Knockback.Base += Value;
	}
}

internal class FlatKnockbackItemAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem gear)
	{
		modifier.Knockback.Flat += Value;
	}
}
	
internal class ChanceToApplyOnFireGearAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem gear)
	{
		modifier.Buffer.Add(BuffID.OnFire, Duration, Value);
	}
}
	
internal class ChanceToApplyArmorShredGearAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem gear)
	{
		modifier.Buffer.Add(ModContent.BuffType<ArmorShredDebuff>(), Duration, Value);
	}
}