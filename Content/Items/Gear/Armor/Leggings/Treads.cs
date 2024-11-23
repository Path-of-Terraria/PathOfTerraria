using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

internal class Treads : Leggings
{
	public override void PostRoll()
	{
		Item.defense = GetItemLevel.Invoke(Item) / 14 + 1;
	}
}