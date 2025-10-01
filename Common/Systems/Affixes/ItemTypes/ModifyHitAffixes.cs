using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Content.Buffs.ElementalBuffs;
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
	
internal class ChanceToApplyOnFireGearAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.Buffer.Add(BuffID.OnFire, Duration, Value * 0.01f);
	}
}
	
internal class ChanceToApplyArmorShredGearAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.Buffer.Add(ModContent.BuffType<ArmorShredDebuff>(), Duration, Value * 0.01f);
	}
}

internal class ChanceToApplyBloodclotItemAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.Buffer.Add(ModContent.BuffType<BloodclotDebuff>(), Duration, Value * 0.01f);
	}
}

internal class ChanceToApplyPoisonItemAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.Buffer.Add(BuffID.Poisoned, Duration, Value * 0.01f);
	}
}

internal class BuffPoisonedHitsAffix : ItemAffix
{
	private sealed class BuffPoisonedHitsAffixModPlayer : ModPlayer
	{
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			base.ModifyHitNPC(target, ref modifiers);
			
			if (target.HasBuff(BuffID.Poisoned) || target.HasBuff<PoisonedDebuff>())
			{
				modifiers.FinalDamage += Player.GetModPlayer<AffixPlayer>().StrengthOf<BuffPoisonedHitsAffix>() * 0.01f;
			}
		}
	}
}

internal class ChanceToApplyShockGearAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.Buffer.Add(ModContent.BuffType<ShockDebuff>(), Duration, Value * 0.01f);
	}
}

internal class BuffShockedEffectAffix : ItemAffix
{
}

internal class ChanceToApplyRootedGearAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.Buffer.Add(ModContent.BuffType<RootedDebuff>(), Duration, Value * 0.01f);
	}
}

internal class ChanceToApplyBleedingItemAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		// TODO: Needs duration modifier?
		modifier.Buffer.Add(BuffID.Bleeding, 5 * 60, Value * 0.01f, (player, npc, _, damage, time) => BleedDebuff.Apply(player, npc, 5 * 60, damage));
	}
}

internal class AddedBleedStackAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<BleedPlayer>().MaxBleedStacks += (int)Math.Round(Value);
	}

	protected override AffixTooltipLine CreateDefaultTooltip(Player player, Item item)
	{
		return base.CreateDefaultTooltip(player, item) with { Value = (int)Math.Round(Value) };
	}
}

internal class BetterBleedAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<BleedPlayer>().BleedEffectiveness += Value / 100f;
	}
}