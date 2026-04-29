using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Content.Buffs.ElementalBuffs;

namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal abstract class ResistItemAffix : ItemAffix
{
	
}

internal class FireResistItemAffix : ResistItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<ElementalPlayer>().Container[ElementType.Fire].Resistance += Value * 0.01f;
	}
}

internal class ColdResistItemAffix : ResistItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<ElementalPlayer>().Container[ElementType.Cold].Resistance += Value * 0.01f;
	}
}

internal class LightningResistItemAffix : ResistItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<ElementalPlayer>().Container[ElementType.Lightning].Resistance += Value * 0.01f;
	}
}

internal class ChaosResistItemAffix : ResistItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<ElementalPlayer>().Container[ElementType.Chaos].Resistance += Value * 0.01f;
	}
}

internal class FireConversionDamage : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		ref ElementalDamage.ElementalDamage damage = ref player.GetModPlayer<ElementalPlayer>().Container[ElementType.Fire].DamageModifier;
		float bonus = player.GetWeaponDamage(item) * (Value / 100f);
		damage = damage.AddModifiers((int)Math.Round(bonus), null);
	}
}

internal class ColdConversionDamage : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		ref ElementalDamage.ElementalDamage damage = ref player.GetModPlayer<ElementalPlayer>().Container[ElementType.Cold].DamageModifier;
		float bonus = player.GetWeaponDamage(item) * (Value / 100f);
		damage = damage.AddModifiers((int)Math.Round(bonus), null);
	}
}

internal class LightningConversionDamage : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		ref ElementalDamage.ElementalDamage damage = ref player.GetModPlayer<ElementalPlayer>().Container[ElementType.Lightning].DamageModifier;
		float bonus = player.GetWeaponDamage(item) * (Value / 100f);
		damage = damage.AddModifiers((int)Math.Round(bonus), null);
	}
}

internal class ChaosConversionDamage : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		ref ElementalDamage.ElementalDamage damage = ref player.GetModPlayer<ElementalPlayer>().Container[ElementType.Chaos].DamageModifier;
		float bonus = player.GetWeaponDamage(item) * (Value / 100f);
		damage = damage.AddModifiers((int)Math.Round(bonus), null);
	}
}

internal class FireFlatDamage : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		ref ElementalDamage.ElementalDamage damage = ref player.GetModPlayer<ElementalPlayer>().Container[ElementType.Fire].DamageModifier;
		damage = damage.AddModifiers((int)Math.Round(Value), null);
	}

	protected override AffixTooltipLine CreateDefaultTooltip(Player player, Item item)
	{
		return base.CreateDefaultTooltip(player, item) with { Value = (int)Math.Round(Value) };
	}
}

internal class ColdFlatDamage : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		ref ElementalDamage.ElementalDamage damage = ref player.GetModPlayer<ElementalPlayer>().Container[ElementType.Cold].DamageModifier;
		damage = damage.AddModifiers((int)Math.Round(Value), null);
	}

	protected override AffixTooltipLine CreateDefaultTooltip(Player player, Item item)
	{
		return base.CreateDefaultTooltip(player, item) with { Value = (int)Math.Round(Value) };
	}
}

internal class LightningFlatDamage : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		ref ElementalDamage.ElementalDamage damage = ref player.GetModPlayer<ElementalPlayer>().Container[ElementType.Lightning].DamageModifier;
		damage = damage.AddModifiers((int)Math.Round(Value), null);
	}

	protected override AffixTooltipLine CreateDefaultTooltip(Player player, Item item)
	{
		return base.CreateDefaultTooltip(player, item) with { Value = (int)Math.Round(Value) };
	}
}

internal class ChaosFlatDamage : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		ref ElementalDamage.ElementalDamage damage = ref player.GetModPlayer<ElementalPlayer>().Container[ElementType.Chaos].DamageModifier;
		damage = damage.AddModifiers((int)Math.Round(Value), null);
	}

	protected override AffixTooltipLine CreateDefaultTooltip(Player player, Item item)
	{
		return base.CreateDefaultTooltip(player, item) with { Value = (int)Math.Round(Value) };
	}
}

internal class ExtraFireDamage : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		ref ElementalDamage.ElementalDamage damage = ref player.GetModPlayer<ElementalPlayer>().Container[ElementType.Fire].DamageModifier;
		float bonus = player.GetWeaponDamage(item) * (Value / 100f);

		damage = damage.AddModifiers((int)Math.Round(bonus), null);
	}
}

internal class ExtraLightningDamage : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		ref ElementalDamage.ElementalDamage damage = ref player.GetModPlayer<ElementalPlayer>().Container[ElementType.Lightning].DamageModifier;
		float bonus = player.GetWeaponDamage(item) * (Value / 100f);

		damage = damage.AddModifiers((int)Math.Round(bonus), null);
	}
}

internal class ExtraColdDamage : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		ref ElementalDamage.ElementalDamage damage = ref player.GetModPlayer<ElementalPlayer>().Container[ElementType.Cold].DamageModifier;
		float bonus = player.GetWeaponDamage(item) * (Value / 100f);

		damage = damage.AddModifiers((int)Math.Round(bonus), null);
	}
}

internal class IgniteChanceAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<IgnitedPlayer>().AddedIgniteChance += Value / 100f;
	}
}

internal class IncreasedIgniteEffectAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<IgnitedPlayer>().IgniteDamage += Value / 100f;
	}
}

internal class AllResistancesAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		foreach (ElementInstance element in player.GetModPlayer<ElementalPlayer>().Container)
		{
			element.Resistance += Value / 100f;
		}
	}
}

