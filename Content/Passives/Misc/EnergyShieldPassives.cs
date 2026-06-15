using PathOfTerraria.Common.Systems.EnergyShield;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class AddedEnergyShieldPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<EnergyShieldPlayer>().AddGlobalEnergyShield(Value * Level);
	}
}

internal class IncreasedEnergyShieldPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<EnergyShieldPlayer>().AddGlobalIncreasedEnergyShield(Value * Level);
	}
}

internal class IncreasedEnergyShieldRechargeRatePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<EnergyShieldPlayer>().AddEnergyShieldRechargeRate(Value * Level);
	}
}
