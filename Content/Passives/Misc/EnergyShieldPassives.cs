using PathOfTerraria.Common.Systems.EnergyShield;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class AddedEnergyShieldPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<EnergyShieldPlayer>().AddGlobalEnergyShield(Value);
	}
}

internal class IncreasedEnergyShieldPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<EnergyShieldPlayer>().AddGlobalIncreasedEnergyShield(Value);
	}
}

internal class IncreasedEnergyShieldRechargeRatePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<EnergyShieldPlayer>().AddEnergyShieldRechargeRate(Value);
	}
}

internal class AddFasterEnergyShieldRechargeStartPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<EnergyShieldPlayer>().AddFasterEnergyShieldRechargeStart(Value);
	}
}
