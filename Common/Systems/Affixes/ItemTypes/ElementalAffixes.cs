using PathOfTerraria.Common.Systems.ElementalDamage;

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
		damage = damage.AddModifiers(null, Value * 0.01f);
	}
}

internal class ColdConversionDamage : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		ref ElementalDamage.ElementalDamage damage = ref player.GetModPlayer<ElementalPlayer>().Container[ElementType.Cold].DamageModifier;
		damage = damage.AddModifiers(null, Value);
	}
}

internal class LightningConversionDamage : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		ref ElementalDamage.ElementalDamage damage = ref player.GetModPlayer<ElementalPlayer>().Container[ElementType.Lightning].DamageModifier;
		damage = damage.AddModifiers(null, Value);
	}
}

internal class ChaosConversionDamage : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		ref ElementalDamage.ElementalDamage damage = ref player.GetModPlayer<ElementalPlayer>().Container[ElementType.Chaos].DamageModifier;
		damage = damage.AddModifiers(null, Value);
	}
}