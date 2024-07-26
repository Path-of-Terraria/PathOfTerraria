using PathOfTerraria.Content.Buffs;
using Terraria.ID;

namespace PathOfTerraria.Core.Systems.Affixes.ItemTypes;

internal class PiercingItemAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, Item gear)
	{
		modifier.ArmorPenetration.Base += Value;
	}
}

internal class AddedKnockbackItemAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, Item gear)
	{
		modifier.Knockback.Base += Value;
	}
}

internal class IncreasedKnockbackItemAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, Item gear)
	{
		modifier.Knockback *= Value / 100;
	}
}

internal class FlatKnockbackItemAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, Item gear)
	{
		modifier.Knockback.Flat += Value;
	}
}
	
internal class ChanceToApplyOnFireGearAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, Item gear)
	{
		modifier.Buffer.Add(BuffID.OnFire, Duration, Value);
	}
}
	
internal class ChanceToApplyArmorShredGearAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, Item gear)
	{
		modifier.Buffer.Add(ModContent.BuffType<ArmorShredDebuff>(), Duration, Value);
	}
}