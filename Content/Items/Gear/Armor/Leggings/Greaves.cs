using PathOfTerraria.Content.Items.Gear.Affixes.ArmorAffixes;
using PathOfTerraria.Content.Items.Gear.Affixes;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal class Greaves : Leggings
{
	public override List<Affix> GenerateImplicits()
	{
		return new List<Affix>() { Affix.CreateAffix<MovementSpeed>(0.5f) };
	}
	public override void PostRoll()
	{
		Item.defense = ItemLevel / 14 + 1;
	}
}