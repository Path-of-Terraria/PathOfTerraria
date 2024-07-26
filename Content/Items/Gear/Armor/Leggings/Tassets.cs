using PathOfTerraria.Core.Items.Hooks;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal class Tassets : Leggings
{
	public override void PostRoll(Item item)
	{
		Item.defense = IItemLevelControllerItem.GetLevel(item) / 14 + 1;
	}
}