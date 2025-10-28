using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class AddedFireDamageConversionPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		ElementalPlayer elemental = player.GetModPlayer<ElementalPlayer>();
		elemental.Container[ElementType.Fire].DamageModifier.AddModifiers(null, Value / 100f);
	}
}

internal class AddedColdDamageConversionPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		ElementalPlayer elemental = player.GetModPlayer<ElementalPlayer>();
		elemental.Container[ElementType.Cold].DamageModifier.AddModifiers(null, Value);
	}
}

internal class AddedLightningDamageConversionPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		ElementalPlayer elemental = player.GetModPlayer<ElementalPlayer>();
		elemental.Container[ElementType.Lightning].DamageModifier.AddModifiers(null, Value / 100f);
	}
}