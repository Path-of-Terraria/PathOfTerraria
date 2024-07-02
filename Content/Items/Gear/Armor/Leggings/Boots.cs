using PathOfTerraria.Core.Systems.Affixes;
using System.Collections.Generic;
using PathOfTerraria.Core.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal class Boots : Leggings
{
	public override List<ItemAffix> GenerateImplicits()
	{
		return new List<ItemAffix>() { (ItemAffix)Affix.CreateAffix<MovementSpeed>(0.8f) };
	}

	public override void PostRoll()
	{
		Item.defense = ItemLevel / 18;
	}
}