using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Gear.Armor.Helmet;

[AutoloadEquip(EquipType.Head)]
internal sealed class AcolyteHood : EnergyShieldHelmet
{
	protected override int MinimumDropItemLevel => 25;
	protected override int MaximumDropItemLevel => 50;

	protected override int MinimumEnergyShield => 10;
	protected override int MaximumEnergyShield => 20;
}
