using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal sealed class EtherealGreaves : EnergyShieldLeggings
{
	protected override int MinimumDropItemLevel => 71;
	protected override int MaximumDropItemLevel => 85;

	protected override int MinimumEnergyShield => 36;
	protected override int MaximumEnergyShield => 46;
}
