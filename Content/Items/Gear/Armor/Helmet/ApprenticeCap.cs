using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Gear.Armor.Helmet;

[AutoloadEquip(EquipType.Head)]
internal sealed class ApprenticeCap : EnergyShieldHelmet
{
	protected override int MinimumDropItemLevel => 1;
	protected override int MaximumDropItemLevel => 35;

	protected override int MinimumEnergyShield => 3;
	protected override int MaximumEnergyShield => 10;
}
