using PathOfTerraria.Common.Systems.EnergyShield;

namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal class MaximumEnergyShieldAffix : ItemAffix
{
	public MaximumEnergyShieldAffix()
	{
		Round = true;
	}

	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		if (EnergyShieldItem.IsGlobalEnergyShieldSource(item))
		{
			player.GetModPlayer<EnergyShieldPlayer>().AddGlobalEnergyShield(Value);
		}
	}
}

internal class IncreasedEnergyShieldAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		if (EnergyShieldItem.IsGlobalEnergyShieldSource(item))
		{
			player.GetModPlayer<EnergyShieldPlayer>().AddGlobalIncreasedEnergyShield(Value);
		}
	}
}

internal class EnergyShieldRechargeRateAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<EnergyShieldPlayer>().AddEnergyShieldRechargeRate(Value);
	}
}

internal class FasterEnergyShieldRechargeStartAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<EnergyShieldPlayer>().AddFasterEnergyShieldRechargeStart(Value);
	}
}
