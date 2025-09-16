using PathOfTerraria.Common.Systems.Charges;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Misc.Charges;

//+% Chance to gain an Aegis Charge on Kill
internal class AegisChargeChanceOnKillPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<AegisChargePlayer>().ChargeGainChance += Value * Level;
	}
}

//+% Max Health per Aegis Charge
internal class AegisChargePercentMaxHealthPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		// Instead of directly modifying statLifeMax2, update the AegisChargePlayer's percent bonus
		var aegisPlayer = player.GetModPlayer<AegisChargePlayer>();
		aegisPlayer.AegisPercentHealthBonus += (Value / 100.0f) * Level;
	}
}

//+X to Maximum Aegis Charges
internal class AegisChargeMaxChargesPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<AegisChargePlayer>().MaxCharges += Value * Level;
	}
}
