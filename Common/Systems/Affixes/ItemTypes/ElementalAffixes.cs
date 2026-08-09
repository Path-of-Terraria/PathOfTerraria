using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Content.Buffs.ElementalBuffs;

namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal abstract class ResistItemAffix : ItemAffix
{
	protected ResistItemAffix()
	{
		Round = true;
	}
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

internal interface IElementalDamageAffix
{
	ElementType ElementType { get; }
	int DamageBonus { get; }
	float DamageConversion { get; }
}

internal abstract class ElementalConversionDamageAffix(ElementType elementType) : ItemAffix, IElementalDamageAffix
{
	public ElementType ElementType { get; } = elementType;
	public int DamageBonus => 0;
	public float DamageConversion => Value / 100f;

	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		ref ElementalDamage.ElementalDamage damage = ref player.GetModPlayer<ElementalPlayer>().Container[ElementType].DamageModifier;
		damage = damage.AddModifiers(null, DamageConversion);
	}
}

internal abstract class ElementalFlatDamageAffix(ElementType elementType) : ItemAffix, IElementalDamageAffix
{
	public ElementType ElementType { get; } = elementType;
	public int DamageBonus => (int)Math.Round(Value);
	public float DamageConversion => 0f;

	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		ref ElementalDamage.ElementalDamage damage = ref player.GetModPlayer<ElementalPlayer>().Container[ElementType].DamageModifier;
		damage = damage.AddModifiers(DamageBonus, null);
	}

	protected override AffixTooltipLine CreateDefaultTooltip(Player player, Item item)
	{
		return base.CreateDefaultTooltip(player, item) with { Value = DamageBonus };
	}
}

internal abstract class ExtraElementalDamageAffix(ElementType elementType) : ItemAffix, IElementalDamageAffix
{
	public ElementType ElementType { get; } = elementType;
	public int DamageBonus => 0;
	public float DamageConversion => Value / 100f;

	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.Damage += DamageConversion;

		ref ElementalDamage.ElementalDamage damage = ref player.GetModPlayer<ElementalPlayer>().Container[ElementType].DamageModifier;
		damage = damage.AddModifiers(null, DamageConversion);
	}
}

internal class FireConversionDamage() : ElementalConversionDamageAffix(ElementType.Fire) { }
internal class ColdConversionDamage() : ElementalConversionDamageAffix(ElementType.Cold) { }
internal class LightningConversionDamage() : ElementalConversionDamageAffix(ElementType.Lightning) { }
internal class ChaosConversionDamage() : ElementalConversionDamageAffix(ElementType.Chaos) { }

internal class FireFlatDamage() : ElementalFlatDamageAffix(ElementType.Fire) { }
internal class ColdFlatDamage() : ElementalFlatDamageAffix(ElementType.Cold) { }
internal class LightningFlatDamage() : ElementalFlatDamageAffix(ElementType.Lightning) { }
internal class ChaosFlatDamage() : ElementalFlatDamageAffix(ElementType.Chaos) { }

internal class ExtraFireDamage() : ExtraElementalDamageAffix(ElementType.Fire) { }
internal class ExtraLightningDamage() : ExtraElementalDamageAffix(ElementType.Lightning) { }
internal class ExtraColdDamage() : ExtraElementalDamageAffix(ElementType.Cold) { }
internal class ExtraChaosDamage() : ExtraElementalDamageAffix(ElementType.Chaos) { }

internal class IgniteChanceAffix : ItemAffix
{
	public IgniteChanceAffix()
	{
		Round = true;
	}

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
	public AllResistancesAffix()
	{
		Round = true;
	}

	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		foreach (ElementInstance element in player.GetModPlayer<ElementalPlayer>().Container)
		{
			element.Resistance += Value / 100f;
		}
	}
}

