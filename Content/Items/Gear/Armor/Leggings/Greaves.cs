using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

internal class Greaves : Leggings
{
	public override List<ItemAffix> GenerateImplicits()
	{
		return [(ItemAffix)Affix.CreateAffix<MovementSpeed>(0.5f)];
	}

	public override void PostRoll()
	{
		Item.defense = GetItemLevel.Invoke(Item) / 14 + 1;
	}
}