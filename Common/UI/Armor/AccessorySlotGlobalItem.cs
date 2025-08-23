using PathOfTerraria.Content.Items.Gear.Amulets;
using PathOfTerraria.Content.Items.Gear.Offhands;
using PathOfTerraria.Content.Items.Gear.Rings;

namespace PathOfTerraria.Common.UI.Armor;

public sealed class AccessorySlotGlobalItem : GlobalItem
{
	public override bool InstancePerEntity => true;

	public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded)
	{
		bool result = slot switch
		{
			(int)EquipmentSlot.AccessorySlot1 => IsNormalAccessory(item),
			(int)EquipmentSlot.AccessorySlot2 => IsNormalAccessory(item),
			(int)EquipmentSlot.Wings => item.wingSlot > 0,
			(int)EquipmentSlot.Necklace => item.ModItem is Amulet,
			(int)EquipmentSlot.Offhand => item.ModItem is Offhand,
			(int)EquipmentSlot.Ring1 => item.ModItem is Ring,
			(int)EquipmentSlot.Ring2 => item.ModItem is Ring,
			_ => true
		};

		return result;
	}

	public static bool IsNormalAccessory(Item item)
	{
		return item is not null && 
		       item.accessory && 
		       item.wingSlot <= 0 &&  // Exclude wings
		       item.ModItem is not Offhand && 
		       item.ModItem is not Ring && 
		       item.ModItem is not Amulet;
	}
	
	private enum EquipmentSlot
	{
		AccessorySlot1 = 3,
		Wings = 4,
		Necklace = 5,
		Offhand = 6,
		Ring1 = 7,
		Ring2 = 8,
		AccessorySlot2 = 9,
	}
}