using PathOfTerraria.Common.Systems.Charges;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Misc.Charges;

//+% Chance to gain a Haste Charge on Kill
internal class HasteChargeChanceOnKillPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<HasteChargePlayer>().ChargeGainChance += Value * Level;
	}
}

//+% Projectile Speed from Haste Charges
internal class HasteChargeProjectileSpeedPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		var hastePlayer = player.GetModPlayer<HasteChargePlayer>();
		hastePlayer.HasteProjectileSpeedBonus += (Value / 100.0f) * Level;
	}
}

//+X to Maximum Haste Charges
internal class HasteChargeMaxChargesPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<HasteChargePlayer>().MaxCharges += (int)(Value * Level);
	}
}
