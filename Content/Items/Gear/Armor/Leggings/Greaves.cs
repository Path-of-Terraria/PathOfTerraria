﻿using System.Collections.Generic;
using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.Affixes.ItemTypes.ArmorAffixes;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal class Greaves : Leggings
{
	public override List<ItemAffix> GenerateImplicits()
	{
		return [(ItemAffix)Affix.CreateAffix<MovementSpeed>(0.5f)];
	}

	public override void PostRoll()
	{
		Item.defense = ItemLevel / 14 + 1;
	}
}