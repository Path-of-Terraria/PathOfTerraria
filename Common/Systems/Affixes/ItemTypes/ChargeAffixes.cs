using PathOfTerraria.Common.Systems.Charges;

namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal class PowerChargeChanceOnKillAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{       
		var powerChargePlayer = player.GetModPlayer<PowerChargePlayer>();
		powerChargePlayer.ChargeGainChance = Value;
	}
}

internal class FrenzyChargeChanceOnKillAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{       
		var frenzyChargePlayer = player.GetModPlayer<FrenzyChargePlayer>();
		frenzyChargePlayer.ChargeGainChance = Value;
	}
}

internal class EnduranceChargeChanceOnKillAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{       
		var enduranceChargePlayer = player.GetModPlayer<EnduranceChargePlayer>();
		enduranceChargePlayer.ChargeGainChance = Value ;
	}
}
