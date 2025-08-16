using PathOfTerraria.Common.Systems.Charges;

namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal class FocusChargeChanceOnKillAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{       
		var FocusChargePlayer = player.GetModPlayer<FocusChargePlayer>();
		FocusChargePlayer.ChargeGainChance = Value;
	}
}

internal class HasteChargeChanceOnKillAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{       
		var HasteChargePlayer = player.GetModPlayer<HasteChargePlayer>();
		HasteChargePlayer.ChargeGainChance = Value;
	}
}

internal class AegisChargeChanceOnKillAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{       
		var AegisChargePlayer = player.GetModPlayer<AegisChargePlayer>();
		AegisChargePlayer.ChargeGainChance = Value ;
	}
}
