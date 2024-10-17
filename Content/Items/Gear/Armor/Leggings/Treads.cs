using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal class Treads : Leggings
{
	public override void PostRoll()
	{
		Item.defense = GetItemLevel.Invoke(Item) / 14 + 1;
	}
}