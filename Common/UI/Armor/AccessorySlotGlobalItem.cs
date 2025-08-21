using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Content.Items.Gear.Amulets;
using PathOfTerraria.Content.Items.Gear.Offhands;
using PathOfTerraria.Content.Items.Gear.Rings;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Armor;

public sealed class AccessorySlotGlobalItem : GlobalItem
{
	public override bool InstancePerEntity => true;

	private bool equipped = false;

	public override void Load()
	{
		On_ItemSlot.AccessorySwap += OnAccessorySwap;
	}

	private static bool OnAccessorySwap(On_ItemSlot.orig_AccessorySwap orig, Player player, Item item, ref Item result)
	{
		bool origResult = orig(player, item, ref result);

		if (!origResult && player.TryGetModPlayer(out ExtraAccessoryModPlayer accPlayer))
		{
			for (int i = 0; i < accPlayer.CustomAccessorySlots.Length; i++)
			{
				ref Item slotItem = ref accPlayer.CustomAccessorySlots[i];
				int virtualIndex = ExtraAccessoryModPlayer.GetCustomSlotVirtualIndex(i);

				if (accPlayer.IsCustomSlotActive(virtualIndex) && (slotItem.IsAir || (item.type == slotItem.type && item.netID == slotItem.netID)))
				{
					Utils.Swap(ref result, ref slotItem);
					return true;
				}
			}
		}

		return origResult;
	}

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