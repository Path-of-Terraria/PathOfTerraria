using System.Collections.Generic;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal class Greaves : Leggings
{
	public override List<ItemAffix> GenerateImplicits(Item item)
	{
		return [(ItemAffix)Affix.CreateAffix<MovementSpeed>(0.5f)];
	}

	public override void PostRoll(Item item)
	{
		Item.defense = GetItemLevel.Invoke(item) / 14 + 1;
	}
}