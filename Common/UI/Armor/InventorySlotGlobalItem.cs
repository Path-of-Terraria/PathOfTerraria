using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Armor;

public sealed class InventorySlotGlobalItem : GlobalItem
{
	public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded)
	{
		var result = false;
		
		switch (slot)
		{
			case 4:
				result = item.wingSlot > 0;
				break;
			case 5:
			case 6:
			case 7:
			case 8:
				result = item.accessory && item.wingSlot <= 0;
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