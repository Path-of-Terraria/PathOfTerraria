using PathOfTerraria.Core.Items.Hooks;

namespace PathOfTerraria.Content.Items.Gear.Armor.Helmet;

[AutoloadEquip(EquipType.Head)]
internal class Visor : Helmet
{
	public override void PostRoll(Item item)
	{
		Item.defense = IItemLevelControllerItem.GetLevel(item) / 10 + 1;
	}
}