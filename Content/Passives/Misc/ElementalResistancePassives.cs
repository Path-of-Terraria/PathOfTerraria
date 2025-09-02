using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class AddedFireResistancePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		var elemental = player.GetModPlayer<ElementalPlayer>();
		elemental.FireResistance += Value / 100f;
	}
}

//AddedColdResistancePassive
internal class AddedColdResistancePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		var elemental = player.GetModPlayer<ElementalPlayer>();
		elemental.ColdResistance += Value / 100f;
	}
}

//AddedLightningResistancePassive
internal class AddedLightningResistancePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		var elemental = player.GetModPlayer<ElementalPlayer>();
		elemental.LightningResistance += Value / 100f;
	}
}

//AddedAllResistancePassive
internal class AddedAllResistancePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		var elemental = player.GetModPlayer<ElementalPlayer>();
		float resistanceBonus = Value / 100f;
		
		elemental.FireResistance += resistanceBonus;
		elemental.ColdResistance += resistanceBonus;
		elemental.LightningResistance += resistanceBonus;
	}
}