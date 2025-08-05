using PathOfTerraria.Content.Items.Gear.Amulets;
using PathOfTerraria.Content.Items.Gear.Offhands;
using PathOfTerraria.Content.Items.Gear.Rings;

namespace PathOfTerraria.Common.UI.Armor;

public sealed class AccessorySlotGlobalItem : GlobalItem
{
	public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded)
	{
		bool result = slot switch
		{
			(int)EquipmentSlot.LeftAccessorySlot => IsNormalAccessory(item),
			(int)EquipmentSlot.Wings => item.wingSlot > 0,
			(int)EquipmentSlot.Necklace => item.ModItem is Amulet,
			(int)EquipmentSlot.Offhand => item.ModItem is Offhand,
			(int)EquipmentSlot.Ring1 or (int)EquipmentSlot.Ring2 => item.ModItem is Ring,
			(int)EquipmentSlot.RightAccessorySlot => IsNormalAccessory(item),
			_ => true
		};

		return result;
	}
	
	private bool IsNormalAccessory(Item item)
	{
		return item is not null && item.accessory && item.ModItem is not Offhand && item.ModItem is not Ring && item.ModItem is not Amulet;
	}
	
	private enum EquipmentSlot
	{
		LeftAccessorySlot = 3,
		Wings = 4,
		Necklace = 5,
		Offhand = 6,
		Ring1 = 7,
		Ring2 = 8,
		RightAccessorySlot = 9
	}
}