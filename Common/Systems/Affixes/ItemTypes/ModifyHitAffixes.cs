﻿using PathOfTerraria.Content.Buffs;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal class PiercingItemAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.ArmorPenetration.Base += Value;
	}
}

internal class AddedKnockbackItemAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.Knockback.Base += Value;
	}
}

internal class IncreasedKnockbackItemAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.Knockback *= Value / 100;
	}
}

internal class FlatKnockbackItemAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.Knockback.Flat += Value;
	}
}
	
internal class ChanceToApplyOnFireGearAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.Buffer.Add(BuffID.OnFire, Duration, Value);
	}
}
	
internal class ChanceToApplyArmorShredGearAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.Buffer.Add(ModContent.BuffType<ArmorShredBuff>(), Duration, Value);
	}
}

internal class ChanceToApplyBloodclotItemAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.Buffer.Add(ModContent.BuffType<BloodclotDebuff>(), Duration, Value);
	}
}

internal class ChanceToApplyPoisonItemAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.Buffer.Add(BuffID.Poisoned, Duration, Value);
	}
}

internal class BuffPoisonedHitsAffix : ItemAffix
{
	private sealed class BuffPoisonedHitsAffixModPlayer : ModPlayer
	{
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			base.ModifyHitNPC(target, ref modifiers);
			
			if (target.HasBuff(BuffID.Poisoned))
			{
				modifiers.FinalDamage += Player.GetModPlayer<AffixPlayer>().StrengthOf<BuffPoisonedHitsAffix>();
			}
		}
	}
}