using System.Collections.Generic;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal class Boots : Leggings
{
	public override void PostRoll()
	{
		Item.defense = ItemLevel / 18;
	}
}