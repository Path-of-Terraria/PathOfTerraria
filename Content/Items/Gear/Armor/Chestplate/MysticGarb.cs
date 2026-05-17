using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Gear.Armor.Chestplate;

[AutoloadEquip(EquipType.Body)]
internal sealed class MysticGarb : EnergyShieldChestplate
{
	protected override int MinimumDropItemLevel => 40;
	protected override int MaximumDropItemLevel => 65;

	protected override int MinimumEnergyShield => 38;
	protected override int MaximumEnergyShield => 58;
}
