using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Items.Gear.Armor.Chestplate;

[AutoloadEquip(EquipType.Body)]
internal class PyralisHeart : Chestplate
{
	public override List<ItemAffix> GenerateImplicits()
	{
		return
		[
			(ItemAffix)Affix.CreateAffix<BaseLifeAffix>(150),
			(ItemAffix)Affix.CreateAffix<FireResistItemAffix>(20),
			(ItemAffix)Affix.CreateAffix<MovementSpeedAffix>(-5),
		];
	}

	public override void PostRoll()
	{
		Item.defense = 30;
	}
}
