using PathOfTerraria.Content.Items.Gear.Amulets;
using PathOfTerraria.Content.Items.Gear.Offhands;
using PathOfTerraria.Content.Items.Gear.Rings;

namespace PathOfTerraria.Common.UI.Armor;

public sealed class InventorySlotGlobalItem : GlobalItem
{
	public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded)
	{
		bool result = false;

		switch (slot)
		{
			case 4:
				result = item.wingSlot > 0;
				break;
			case 5:
				result = item.ModItem is Amulet;
				break;
			case 6:
				result = item.ModItem is Offhand;
				break;
			case 7:
			case 8:
				result = item.ModItem is Ring;
				break;
			case 14:
			case 15:
			case 16:
			case 17:
			case 18:
				result = true;
				break;
		}

		return result;
	}
}