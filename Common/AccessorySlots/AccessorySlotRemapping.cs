using PathOfTerraria.Content.Items.Gear.Amulets;
using PathOfTerraria.Content.Items.Gear.Offhands;
using PathOfTerraria.Content.Items.Gear.Rings;

namespace PathOfTerraria.Common.AccessorySlots;

/// <summary> Vanilla's equipment slot mappings. In most cases you should refer to <see cref="RemappedEquipSlots"/> instead. </summary>
public enum VanillaEquipSlots
{
	// Logical
	Head = 0,
	Body = 1,
	Legs = 2,
	Accessory1 = 3,
	Accessory2 = 4,
	Accessory3 = 5,
	Accessory4 = 6,
	Accessory5 = 7,
	Accessory6 = 8,
	Accessory7 = 9,
	// Vanity
	VanityHead = 10,
	VanityBody = 11,
	VanityLegs = 12,
	VanityAccessory1 = 13,
	VanityAccessory2 = 14,
	VanityAccessory3 = 15,
	VanityAccessory4 = 16,
	VanityAccessory5 = 17,
	VanityAccessory6 = 18,
	VanityAccessory7 = 19,
}

/// <summary> An enum that maps the mod's reimagined equipment slots to their vanilla indices. </summary>
public enum RemappedEquipSlots
{
	// Logical
	Head = VanillaEquipSlots.Head,
	Body = VanillaEquipSlots.Body,
	Legs = VanillaEquipSlots.Legs,
	Accessory1 = VanillaEquipSlots.Accessory1,
	Wings = VanillaEquipSlots.Accessory2,
	Necklace = VanillaEquipSlots.Accessory3,
	Offhand = VanillaEquipSlots.Accessory4,
	RingOn = VanillaEquipSlots.Accessory5,
	RingOff = VanillaEquipSlots.Accessory6,
	Accessory2 = VanillaEquipSlots.Accessory7,
	// Vanity
	VanityHead = VanillaEquipSlots.VanityHead,
	VanityBody = VanillaEquipSlots.VanityBody,
	VanityLegs = VanillaEquipSlots.VanityLegs,
	VanityAccessory1 = VanillaEquipSlots.VanityAccessory1,
	VanityWings = VanillaEquipSlots.VanityAccessory2,
	VanityNecklace = VanillaEquipSlots.VanityAccessory3,
	VanityOffhand = VanillaEquipSlots.VanityAccessory4,
	VanityRingOn = VanillaEquipSlots.VanityAccessory5,
	VanityRingOff = VanillaEquipSlots.VanityAccessory6,
	VanityAccessory2 = VanillaEquipSlots.VanityAccessory7,
}

public sealed class AccessorySlotRemapping : ModSystem
{
	public override void Load()
	{
		On_Player.IsItemSlotUnlockedAndUsable += Player_IsItemSlotUnlockedAndUsable_Hook;
	}

	public static bool IsNormalAccessory(Item item)
	{
		return item?.IsAir == false
			&& item.accessory
			// Exclude accessores for which specific slots will exist.
			&& item.wingSlot <= 0
			&& item.ModItem is not Offhand
			&& item.ModItem is not Ring
			&& item.ModItem is not Amulet;
	}

	private static bool Player_IsItemSlotUnlockedAndUsable_Hook(On_Player.orig_IsItemSlotUnlockedAndUsable orig, Player self, int slot)
	{
		return true;
	}
}

file sealed class ItemAccessorySlotRemapping : GlobalItem
{
	public override bool InstancePerEntity => true;

	public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded)
	{
		bool result = slot switch
		{
			(int)RemappedEquipSlots.Accessory1 => AccessorySlotRemapping.IsNormalAccessory(item),
			(int)RemappedEquipSlots.Accessory2 => AccessorySlotRemapping.IsNormalAccessory(item),
			(int)RemappedEquipSlots.Wings => item.wingSlot > 0,
			(int)RemappedEquipSlots.Necklace => item.ModItem is Amulet,
			(int)RemappedEquipSlots.Offhand => item.ModItem is Offhand,
			(int)RemappedEquipSlots.RingOn => item.ModItem is Ring,
			(int)RemappedEquipSlots.RingOff => item.ModItem is Ring,
			_ => true
		};

		return result;
	}
}