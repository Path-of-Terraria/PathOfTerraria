using PathOfTerraria.Common.Systems.Charges;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Misc.Charges;

//+% Chance to gain a Focus Charge on Kill
internal class FocusChargeChanceOnKillPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<FocusChargePlayer>().ChargeGainChance += Value * Level;
	}
}

//+% Damage Bonus from Focus Charges
internal class FocusChargeDamageBonusPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		var focusPlayer = player.GetModPlayer<FocusChargePlayer>();
		focusPlayer.FocusChargeDamageBonus = Value / 100.0f * Level;
	}
}

//+X to Maximum Focus Charges
internal class FocusChargeMaxChargesPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<FocusChargePlayer>().MaxCharges += (int)(Value * Level);
	}
}
