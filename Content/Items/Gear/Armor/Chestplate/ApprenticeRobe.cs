using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Gear.Armor.Chestplate;

[AutoloadEquip(EquipType.Body)]
internal sealed class ApprenticeRobe : EnergyShieldChestplate
{
	protected override int MinimumDropItemLevel => 1;
	protected override int MaximumDropItemLevel => 35;

	public override List<ItemAffix> GenerateImplicits()
	{
		return [(ItemAffix)Affix.CreateAffix<MaximumEnergyShieldAffix>(7, 21)];
	}
}
