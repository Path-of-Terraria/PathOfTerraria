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
			(int)EquipmentSlot.AccessorySlot1 => IsNormalAccessory(item),
			(int)EquipmentSlot.AccessorySlot2 => IsNormalAccessory(item),
			(int)EquipmentSlot.AccessorySlot3 => IsNormalAccessory(item),
			(int)EquipmentSlot.AccessorySlot4 => IsNormalAccessory(item) && HasFourthSlotUnlocked(player), // Potential 4th slot
			(int)EquipmentSlot.Wings => item.wingSlot > 0,
			(int)EquipmentSlot.Necklace => item.ModItem is Amulet,
			(int)EquipmentSlot.Offhand => item.ModItem is Offhand,
			(int)EquipmentSlot.Ring1 => item.ModItem is Ring,
			(int)EquipmentSlot.Ring2 => item.ModItem is Ring,
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
		AccessorySlot1 = 3,
		Wings = 4,          // Wings keep their functionality and position
		AccessorySlot2 = 5,
		AccessorySlot3 = 6,
		AccessorySlot4 = 7, // Potential 4th slot - you can conditionally enable this
		Necklace = 8,       // Moved specialized slots after normal accessories
		Offhand = 9,
		Ring1 = 10,
		Ring2 = 11
	}
	
	private bool HasFourthSlotUnlocked(Player player)
	{
		return Main.hardMode; // Unlocked after Wall of Flesh is defeated (hardmode)
	}


}