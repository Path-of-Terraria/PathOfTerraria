using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Gear.Armor.Helmet;

[AutoloadEquip(EquipType.Head)]
internal sealed class MysticCirclet : EnergyShieldHelmet
{
	protected override int MinimumDropItemLevel => 40;
	protected override int MaximumDropItemLevel => 65;

	protected override int MinimumEnergyShield => 19;
	protected override int MaximumEnergyShield => 29;
}
