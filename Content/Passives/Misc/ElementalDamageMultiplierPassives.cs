using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class AddedFireDamageMultiplierPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		ElementalPlayer elemental = player.GetModPlayer<ElementalPlayer>();
		elemental.Container[ElementType.Fire].Multiplier *= 1 + Value / 100f;
	}
}

internal class AddedColdDamageMultiplierPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		ElementalPlayer elemental = player.GetModPlayer<ElementalPlayer>();
		elemental.Container[ElementType.Cold].Multiplier *= 1 + Value / 100f;
	}
}

internal class AddedLightningDamageMultiplierPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		ElementalPlayer elemental = player.GetModPlayer<ElementalPlayer>();
		elemental.Container[ElementType.Lightning].Multiplier *= 1 + Value / 100f;
	}
}