using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal sealed class ApprenticeSlippers : EnergyShieldLeggings
{
	protected override int MinimumDropItemLevel => 1;
	protected override int MaximumDropItemLevel => 35;

	protected override int MinimumEnergyShield => 2;
	protected override int MaximumEnergyShield => 8;
}
