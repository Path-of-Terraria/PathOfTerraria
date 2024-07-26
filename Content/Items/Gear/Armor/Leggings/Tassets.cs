using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal class Tassets : Leggings
{
	public override void PostRoll(Item item)
	{
		Item.defense = GetItemLevel.Invoke(item) / 14 + 1;
	}
}