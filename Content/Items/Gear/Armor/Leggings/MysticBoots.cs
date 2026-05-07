using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal sealed class MysticBoots : EnergyShieldLeggings
{
	protected override int MinimumDropItemLevel => 40;
	protected override int MaximumDropItemLevel => 65;

	public override List<ItemAffix> GenerateImplicits()
	{
		return [(ItemAffix)Affix.CreateAffix<MaximumEnergyShieldAffix>(16, 24)];
	}
}
