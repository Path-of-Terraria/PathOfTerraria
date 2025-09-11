using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class AddedFireResistancePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		ElementalPlayer elemental = player.GetModPlayer<ElementalPlayer>();
		elemental.Container[ElementType.Fire].Resistance += Value / 100f;
	}
}

internal class AddedColdResistancePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		ElementalPlayer elemental = player.GetModPlayer<ElementalPlayer>();
		elemental.Container[ElementType.Cold].Resistance += Value / 100f;
	}
}

internal class AddedLightningResistancePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		ElementalPlayer elemental = player.GetModPlayer<ElementalPlayer>();
		elemental.Container[ElementType.Lightning].Resistance += Value / 100f;
	}
}

internal class AddedChaosResistancePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		ElementalPlayer elemental = player.GetModPlayer<ElementalPlayer>();
		elemental.Container[ElementType.Chaos].Resistance += Value / 100f;
	}
}

internal class AddedAllResistancePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		ElementalPlayer elemental = player.GetModPlayer<ElementalPlayer>();
		float resistanceBonus = Value / 100f;
		
		foreach (ElementInstance element in elemental.Container)
		{
			if (element.IsGeneric)
			{
				element.Resistance += resistanceBonus;
			}
		}
	}
}