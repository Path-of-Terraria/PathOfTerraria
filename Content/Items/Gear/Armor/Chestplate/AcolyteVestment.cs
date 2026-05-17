using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Gear.Armor.Chestplate;

[AutoloadEquip(EquipType.Body)]
internal sealed class AcolyteVestment : EnergyShieldChestplate
{
	protected override int MinimumDropItemLevel => 25;
	protected override int MaximumDropItemLevel => 50;

	protected override int MinimumEnergyShield => 19;
	protected override int MaximumEnergyShield => 39;
}
