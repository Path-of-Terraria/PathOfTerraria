using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal sealed class AcolyteSandals : EnergyShieldLeggings
{
	protected override int MinimumDropItemLevel => 25;
	protected override int MaximumDropItemLevel => 50;

	protected override int MinimumEnergyShield => 8;
	protected override int MaximumEnergyShield => 16;
}
