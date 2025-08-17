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

	public bool IsNormalAccessory(Item item)
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
			return base.CanRightClick(item);
		}

		var accessorySlotGlobal = ModContent.GetInstance<AccessorySlotGlobalItem>();
		return accessorySlotGlobal.IsNormalAccessory(item);
	}

	public override void RightClick(Item item, Player player)
	{

		var extraAccessoryPlayer = player.GetModPlayer<ExtraAccessoryModPlayer>();
		var accessorySlotGlobal = ModContent.GetInstance<AccessorySlotGlobalItem>();
        
		if (!accessorySlotGlobal.IsNormalAccessory(item))
		{
			equipped = false;
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
	
	public override bool ConsumeItem(Item item, Player player)
	{
		// Only consume the item if it was successfully equipped (not swapped)
		return equipped;
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