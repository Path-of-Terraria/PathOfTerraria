using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Content.Items.Gear.Amulets;
using PathOfTerraria.Content.Items.Gear.Offhands;
using PathOfTerraria.Content.Items.Gear.Rings;

namespace PathOfTerraria.Common.UI.Armor;

public sealed class AccessorySlotGlobalItem : GlobalItem
{
	public override bool InstancePerEntity => true;

	private bool equipped = false;
	public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded)
	{
		// Check for duplicates first - applies to all slots
		if (IsItemAlreadyEquipped(item, player, slot))
		{
			return false;
		}

		// Handle custom slots separately
		if (ExtraAccessoryModPlayer.IsCustomSlot(slot))
		{
			return IsNormalAccessory(item);
		}
		
		bool result = slot switch
		{
			(int)EquipmentSlot.AccessorySlot1 => IsNormalAccessory(item),
			(int)EquipmentSlot.AccessorySlot2 => IsNormalAccessory(item),
			(int)EquipmentSlot.AccessorySlot3 => IsNormalAccessory(item),
			(int)EquipmentSlot.AccessorySlot4 => IsNormalAccessory(item),
			(int)EquipmentSlot.Wings => item.wingSlot > 0,
			(int)EquipmentSlot.Necklace => item.ModItem is Amulet,
			(int)EquipmentSlot.Offhand => item.ModItem is Offhand,
			(int)EquipmentSlot.Ring1 => item.ModItem is Ring,
			(int)EquipmentSlot.Ring2 => item.ModItem is Ring,
			_ => true
		};

		return result;
	}
	
	private static bool IsItemAlreadyEquipped(Item itemToCheck, Player player, int targetSlot)
	{
		if (itemToCheck == null || itemToCheck.IsAir)
		{
			return false;
		}

		ExtraAccessoryModPlayer modPlayer = player.GetModPlayer<ExtraAccessoryModPlayer>();

		// Check all vanilla equipment slots (except the target slot we're trying to equip to)
		for (int i = 0; i < player.armor.Length; i++)
		{
			if (i == targetSlot)
			{
				continue; // Skip the slot we're trying to equip to
			}

			if (!player.armor[i].IsAir && ItemsAreSameType(player.armor[i], itemToCheck))
			{
				return true;
			}
		}

		// Check all vanilla dye slots
		for (int i = 0; i < player.dye.Length; i++)
		{
			if (!player.dye[i].IsAir && ItemsAreSameType(player.dye[i], itemToCheck))
			{
				return true;
			}
		}

		// Check custom accessory slots
		for (int i = 0; i < modPlayer.CustomAccessorySlots.Length; i++)
		{
			if (!modPlayer.CustomAccessorySlots[i].IsAir && ItemsAreSameType(modPlayer.CustomAccessorySlots[i], itemToCheck))
			{
				return true;
			}
		}

		// Check custom vanity slots
		for (int i = 0; i < modPlayer.CustomVanitySlots.Length; i++)
		{
			if (!modPlayer.CustomVanitySlots[i].IsAir && ItemsAreSameType(modPlayer.CustomVanitySlots[i], itemToCheck))
			{
				return true;
			}
		}

		// Check custom dye slots
		for (int i = 0; i < modPlayer.CustomDyeSlots.Length; i++)
		{
			if (!modPlayer.CustomDyeSlots[i].IsAir && ItemsAreSameType(modPlayer.CustomDyeSlots[i], itemToCheck))
			{
				return true;
			}
		}

		return false;
	}
	
	//Compares if two items are the same ID. 
	private static bool ItemsAreSameType(Item item1, Item item2)
	{
		return item1.type == item2.type;
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
	
	public override bool CanRightClick(Item item)
	{
		if (!item.accessory)
		{
			return false;
		}

		return IsNormalAccessory(item);
	}

	public override void RightClick(Item item, Player player)
	{
		ExtraAccessoryModPlayer extraAccessoryPlayer = player.GetModPlayer<ExtraAccessoryModPlayer>();
		
		// Check if item is already equipped before trying to equip
		if (IsItemAlreadyEquipped(item, player, -1)) // -1 means we're not targeting a specific slot
		{
			equipped = false;
			item.stack++;
			return;
		}
        
		equipped = false;
		
		// Try vanilla accessory slots first (3 and 9)
		if (player.armor[3].IsAir)
		{
			player.armor[3] = item.Clone();
			equipped = true;
		}
		else if (player.armor[9].IsAir)
		{
			player.armor[9] = item.Clone();
			equipped = true;
		}

		// Try custom slots (only functional slots, indices 0 and 1)
		else if (extraAccessoryPlayer.CustomAccessorySlots[0].IsAir)
		{
			extraAccessoryPlayer.CustomAccessorySlots[0] = item.Clone();
			equipped = true;
		}
		else if (Main.hardMode && extraAccessoryPlayer.CustomAccessorySlots[1].IsAir)
		{
			extraAccessoryPlayer.CustomAccessorySlots[1] = item.Clone();
			equipped = true;
		}
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
		AccessorySlot3 = 20,
		AccessorySlot4 = 21
	}
}