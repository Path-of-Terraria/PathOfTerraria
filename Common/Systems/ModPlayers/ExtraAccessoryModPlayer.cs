using Terraria.ModLoader.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.ModPlayers;

public class ExtraAccessoryModPlayer : ModPlayer
{
	private static readonly int[] CustomFunctionalSlots = { 20, 21, 22, 23, 24, 25 } ;
	
	public Item[] CustomAccessorySlots = new Item[6];
	public Item[] CustomVanitySlots = new Item[6];
	public Item[] CustomDyeSlots = new Item[6];

	
	public override void Initialize()
	{
		for (int i = 0; i < CustomAccessorySlots.Length; i++)
		{
			CustomAccessorySlots[i] = new Item();
			CustomVanitySlots[i] = new Item();
			CustomDyeSlots[i] = new Item();
		}
	}

	public override void SaveData(TagCompound tag)
	{
		for (int i = 0; i < CustomAccessorySlots.Length; i++)
		{
			if (!CustomAccessorySlots[i].IsAir)
			{
				tag[$"CustomAccessory_{i}"] = CustomAccessorySlots[i];
			}
			
			if (!CustomVanitySlots[i].IsAir)
			{
				tag[$"CustomVanity_{i}"] = CustomVanitySlots[i];
			}
			
			if (!CustomDyeSlots[i].IsAir)
			{
				tag[$"CustomDye_{i}"] = CustomDyeSlots[i];
			}
		}
	}

	public override void LoadData(TagCompound tag)
	{
		for (int i = 0; i < CustomAccessorySlots.Length; i++)
		{
			if (tag.TryGet($"CustomAccessory_{i}", out Item item))
			{
				CustomAccessorySlots[i] = item;
			}
			else
			{
				CustomAccessorySlots[i] = new Item();
			}
			
			if (tag.TryGet($"CustomVanity_{i}", out Item vanityItem))
			{
				CustomVanitySlots[i] = vanityItem;
			}
			else
			{
				CustomVanitySlots[i] = new Item();
			}
			
			if (tag.TryGet($"CustomDye_{i}", out Item dyeItem))
			{
				CustomDyeSlots[i] = dyeItem;
			}
			else
			{
				CustomDyeSlots[i] = new Item();
			}
		}
	}

	public override void PostUpdateEquips()
	{
		for (int i = 0; i < CustomAccessorySlots.Length; i++)
		{
			(Item accessory, int virtualIndex) = (CustomAccessorySlots[i], CustomFunctionalSlots[i]);
			if (IsCustomSlotActive(virtualIndex) && !accessory.IsAir)
			{
				Player.ApplyEquipFunctional(accessory, false);
			}
		}

		for (int i = 0; i < CustomVanitySlots.Length; i++)
		{
			(Item accessory, int virtualIndex) = (CustomVanitySlots[i], CustomFunctionalSlots[i]);
			if (IsCustomSlotActive(virtualIndex) && !accessory.IsAir)
			{
				Player.ApplyEquipVanity(accessory);
			}
		}
	}

	public Item GetCustomSlot(int virtualIndex)
	{
		int arrayIndex = GetCustomSlotArrayIndex(virtualIndex);
		
		if (arrayIndex < 0 || arrayIndex >= CustomAccessorySlots.Length)
		{
			throw new ArgumentOutOfRangeException(nameof(virtualIndex), 
				$"Virtual index {virtualIndex} maps to invalid array index {arrayIndex}. Valid range: 0-{CustomAccessorySlots.Length - 1}");
		}
	
		return CustomAccessorySlots[arrayIndex];
	}

	public void SetCustomSlot(int virtualIndex, Item item)
	{
		int arrayIndex = GetCustomSlotArrayIndex(virtualIndex);
		if (arrayIndex >= 0)
		{
			CustomAccessorySlots[arrayIndex] = item;
		}
	}
	
	public Item GetCustomVanitySlot(int virtualIndex)
	{
		int arrayIndex = GetCustomSlotArrayIndex(virtualIndex);
		return arrayIndex >= 0 ? CustomVanitySlots[arrayIndex] : new Item();
	}

	public void SetCustomVanitySlot(int virtualIndex, Item item)
	{
		int arrayIndex = GetCustomSlotArrayIndex(virtualIndex);
		if (arrayIndex >= 0)
		{
			CustomVanitySlots[arrayIndex] = item;
		}
	}

	public Item GetCustomDyeSlot(int virtualIndex)
	{
		int arrayIndex = GetCustomSlotArrayIndex(virtualIndex);
		return arrayIndex >= 0 ? CustomDyeSlots[arrayIndex] : new Item();
	}

	public void SetCustomDyeSlot(int virtualIndex, Item item)
	{
		int arrayIndex = GetCustomSlotArrayIndex(virtualIndex);
		if (arrayIndex >= 0)
		{
			CustomDyeSlots[arrayIndex] = item;
		}
	}

	public static int GetCustomSlotVirtualIndex(int realIndex)
	{
		return CustomFunctionalSlots[realIndex];
	}

	private static int GetCustomSlotArrayIndex(int virtualIndex)
	{
		return Array.IndexOf(CustomFunctionalSlots, virtualIndex);
	}

	public static bool IsCustomSlot(int slot)
	{
		return Array.IndexOf(CustomFunctionalSlots, slot) >= 0;
	}

	public bool IsCustomSlotActive(int virtualIndex)
	{
		int arrayIndex = GetCustomSlotArrayIndex(virtualIndex);
		return arrayIndex switch
		{
			0 => true, // Add one slot by default.
			1 => Main.hardMode, // Add one slot after WoF is defeated.
			_ => false,
		};
	}

	public int CountActiveExtraSlots()
	{
		int numSlots = 0;

		for (int i = 0; i < CustomAccessorySlots.Length; i++)
		{
			if (IsCustomSlotActive(GetCustomSlotVirtualIndex(i)))
			{
				numSlots++;
			}
		}

		return numSlots;
	}
}