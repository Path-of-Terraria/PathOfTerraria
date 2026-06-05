using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Gear.Armor.Chestplate;

[AutoloadEquip(EquipType.Body)]
internal sealed class SpectralShroud : EnergyShieldChestplate
{
	protected override int MinimumDropItemLevel => 55;
	protected override int MaximumDropItemLevel => 80;

	protected override int MinimumEnergyShield => 58;
	protected override int MaximumEnergyShield => 83;
}
