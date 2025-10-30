using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class AddedFireDamageConversionPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		ElementalPlayer elemental = player.GetModPlayer<ElementalPlayer>();
		ref ElementalDamage self = ref elemental.Container[ElementType.Fire].DamageModifier;
		self = self.AddModifiers(null, Value / 100f);
	}
}

internal class AddedColdDamageConversionPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		ElementalPlayer elemental = player.GetModPlayer<ElementalPlayer>();
		ref ElementalDamage self = ref elemental.Container[ElementType.Cold].DamageModifier;
		self = self.AddModifiers(null, Value / 100f);
	}
}

internal class AddedLightningDamageConversionPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		ElementalPlayer elemental = player.GetModPlayer<ElementalPlayer>();
		ref ElementalDamage self = ref elemental.Container[ElementType.Lightning].DamageModifier;
		self = self.AddModifiers(null, Value / 100f);
	}
}