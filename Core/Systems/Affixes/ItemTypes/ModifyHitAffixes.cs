using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Core.Events;
using Terraria.ID;

namespace PathOfTerraria.Core.Systems.Affixes.ItemTypes;

internal class PiercingItemAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, PoTItem gear)
	{
		modifier.ArmorPenetration.Base += Value;
	}
}

internal class AddedKnockbackItemAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, PoTItem gear)
	{
		modifier.Knockback.Base += Value;
	}
}

internal class IncreasedKnockbackItemAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, PoTItem gear)
	{
		modifier.Knockback *= Value / 100;
	}
}

internal class FlatKnockbackItemAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, PoTItem gear)
	{
		modifier.Knockback.Flat += Value;
	}
}
	
internal class ChanceToApplyOnFireGearAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, PoTItem gear)
	{
		modifier.Buffer.Add(BuffID.OnFire, Duration, Value);
	}
}
	
internal class ChanceToApplyArmorShredGearAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, PoTItem gear)
	{
		modifier.Buffer.Add(ModContent.BuffType<ArmorShredDebuff>(), Duration, Value);
	}
}

internal class ChanceToApplyBloodclotItemAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, PoTItem gear)
	{
		modifier.Buffer.Add(ModContent.BuffType<BloodclotDebuff>(), Duration, Value);
	}
}

internal class ChanceToApplyPoisonItemAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, PoTItem gear)
	{
		modifier.Buffer.Add(BuffID.Poisoned, Duration, Value);
	}
}

internal class BuffPoisonedHitsAffix : ItemAffix
{
	public override void OnLoad()
	{
		PathOfTerrariaPlayerEvents.ModifyHitNPCEvent += BoostPoisonedDamage;
	}

	private void BoostPoisonedDamage(Player self, NPC target, ref NPC.HitModifiers modifiers)
	{
		if (target.HasBuff(BuffID.Poisoned))
		{
			modifiers.FinalDamage += self.GetModPlayer<AffixPlayer>().StrengthOf<BuffPoisonedHitsAffix>();
		}
	}
}