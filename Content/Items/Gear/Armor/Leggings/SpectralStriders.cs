using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal sealed class SpectralStriders : EnergyShieldLeggings
{
	protected override int MinimumDropItemLevel => 55;
	protected override int MaximumDropItemLevel => 80;

	public override List<ItemAffix> GenerateImplicits()
	{
		return [(ItemAffix)Affix.CreateAffix<MaximumEnergyShieldAffix>(25, 35)];
	}
}
