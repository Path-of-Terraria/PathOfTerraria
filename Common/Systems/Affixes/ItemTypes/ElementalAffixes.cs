using PathOfTerraria.Common.Systems.ElementalDamage;

namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal abstract class ResistItemAffix : ItemAffix
{
	
}

internal class FireResistItemAffix : ResistItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<ElementalPlayer>().Container.FireResistance += Value * 0.01f;
	}
}

internal class ColdResistItemAffix : ResistItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<ElementalPlayer>().Container.ColdResistance += Value * 0.01f;
	}
}

internal class LightningResistItemAffix : ResistItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<ElementalPlayer>().Container.LightningResistance += Value * 0.01f;
	}
}

internal class FireConversionDamage : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		ref ElementalDamage.ElementalDamage damage = ref player.GetModPlayer<ElementalPlayer>().Container.FireDamageModifier;
		damage = damage.AddModifiers(null, Value * 0.01f);
	}
}

internal class ColdConversionDamage : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		ref ElementalDamage.ElementalDamage damage = ref player.GetModPlayer<ElementalPlayer>().Container.ColdDamageModifier;
		damage = damage.AddModifiers(null, Value);
	}
}

internal class LightningConversionDamage : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		ref ElementalDamage.ElementalDamage damage = ref player.GetModPlayer<ElementalPlayer>().Container.LightningDamageModifier;
		damage = damage.AddModifiers(null, Value);
	}
}