using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Gear.Armor.Chestplate;

[AutoloadEquip(EquipType.Body)]
internal sealed class EtherealMantle : EnergyShieldChestplate
{
	protected override int MinimumDropItemLevel => 71;
	protected override int MaximumDropItemLevel => 85;

	public override List<ItemAffix> GenerateImplicits()
	{
		return [(ItemAffix)Affix.CreateAffix<MaximumEnergyShieldAffix>(85, 110)];
	}
}
