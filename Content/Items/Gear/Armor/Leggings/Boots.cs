using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.Affixes.Affixes.GearTypes.ArmorAffixes;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal class Boots : Leggings
{
	public override List<GearAffix> GenerateImplicits()
	{
		return new List<GearAffix>() { (GearAffix)Affix.CreateAffix<MovementSpeed>(0.8f) };
	}

	public override void PostRoll()
	{
		Item.defense = ItemLevel / 18;
	}
}