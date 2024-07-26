using PathOfTerraria.Core.Systems.Affixes;
using System.Collections.Generic;
using PathOfTerraria.Core.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal class Boots : Leggings
{
	public override void PostRoll(Item item)
	{
		Item.defense = GetItemLevel.Invoke(item) / 18;
	}
}