using PathOfTerraria.Content.Items.Gear.Affixes;
using PathOfTerraria.Content.Items.Gear.Affixes.ArmorAffixes;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal class Boots : Leggings
{
	public override List<Affix> GenerateImplicits()
	{
		return new List<Affix>() { Affix.CreateAffix<MovementSpeed>(0.8f) };
	}

	public override void PostRoll()
	{
		Item.defense = ItemLevel / 18;
	}
}