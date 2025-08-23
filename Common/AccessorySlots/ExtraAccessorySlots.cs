using Terraria.ModLoader.Default;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.AccessorySlots;

/// <summary>
/// A simple extra accessory slot originating from this mod.
/// </summary>
[Autoload(false)]
public sealed class ExtraAccessorySlot : ModAccessorySlot
{
	public required int IndexInMod { get; init; }

	public override string Name => $"{GetType().Name}" + IndexInMod;

	public override bool IsEnabled()
	{
		return ExtraAccessorySlots.IsLocalSlotActive(IndexInMod);
	}

	public override bool CanAcceptItem(Item checkItem, AccessorySlotType context)
	{
		return ExtraAccessorySlots.CanLocalSlotAcceptItem(IndexInMod, checkItem, context);
	}
}

/// <summary>
/// System that keeps track of <see cref="ModAccessorySlot"/>s added by the mod.
/// </summary>
public sealed class ExtraAccessorySlots : ModSystem
{
	public static int MaxAmount => 6; // Keep it 6 or higher to migrate old data.
	private static ExtraAccessorySlot[] slots;

	public override void Load()
	{
		slots = new ExtraAccessorySlot[MaxAmount];

		for (int i = 0; i < slots.Length; i++)
		{
			slots[i] = new ExtraAccessorySlot
			{
				IndexInMod = i,
			};
			Mod.AddContent(slots[i]);
		}
	}

	/// <summary> Returns whether a potentially foreign modded accessory slot is allowed to be shown. </summary>
	public static bool IsModAccessorySlotAllowed(ModAccessorySlot slot)
	{
		// You can use this method to forbid certain mods' slots.
		return slot.Mod.Name switch
		{
			// Allow our mod.
			nameof(PathOfTerraria) => true,

			// Forbid WingSlot's slots, since we already have a wing slot in our mod.
			"WingSlot" => false,

			// Default to true.
			_ => true,
		};
	}

	/// <summary> Determines whether the given accessory slot will be visible in this mod's inventory. </summary>
	public static bool IsModAccessorySlotVisible(ModAccessorySlot slot)
	{
		return (slot.IsEnabled() || slot.IsVisibleWhenNotEnabled()) && !slot.IsHidden() && IsModAccessorySlotAllowed(slot);
	}

	public static int CountActiveAndAllowedExtraAccessorySlots(Player player)
	{
		int amount = 0;
		AccessorySlotLoader accessoryLoader = LoaderManager.Get<AccessorySlotLoader>();
		ModAccessorySlotPlayer accessoryPlayer = player.GetModPlayer<ModAccessorySlotPlayer>();

		for (int i = 0; i < accessoryPlayer.SlotCount; i++)
		{
			if (IsModAccessorySlotVisible(accessoryLoader.Get(i, player)))
			{
				amount++;
			}
		}

		return amount;
	}

	/// <summary> Returns the custom slot that originates from this mod and has the provided local index. </summary>
	public static ExtraAccessorySlot GetByLocalIndex(int localIndex)
	{
		return slots[localIndex];
	}

	/// <summary> Calculates whether a custom slot originating from this mod should be enabled. </summary>
	public static bool IsLocalSlotActive(int localIndex)
	{
		return localIndex switch
		{
			0 => true, // Add one slot by default.
			1 => Main.hardMode, // Add one slot after WoF is defeated.
			_ => false,
		};
	}

	public static bool CanLocalSlotAcceptItem(int localIndex, Item item, AccessorySlotType context)
	{
		_ = localIndex;

		if (context == AccessorySlotType.VanitySlot && !item.FitsAccessoryVanitySlot)
		{
			return false;
		}

		if (context != AccessorySlotType.DyeSlot && !AccessorySlotGlobalItem.IsNormalAccessory(item))
		{
			return false;
		}

		return true;
	}
}

[Obsolete("Now exists solely to migrate old save data.", true)]
public class ExtraAccessoryModPlayer : ModPlayer
{
	const int NumLegacySlots = 6;

	public override void LoadData(TagCompound tag)
	{
		for (int i = 0; i < NumLegacySlots; i++)
		{
			if (tag.TryGet($"CustomAccessory_{i}", out Item item))
			{
				ExtraAccessorySlots.GetByLocalIndex(i).FunctionalItem = item;
			}

			if (tag.TryGet($"CustomVanity_{i}", out Item vanityItem))
			{
				ExtraAccessorySlots.GetByLocalIndex(i).VanityItem = vanityItem;
			}

			if (tag.TryGet($"CustomDye_{i}", out Item dyeItem))
			{
				ExtraAccessorySlots.GetByLocalIndex(i).DyeItem = dyeItem;
			}
		}
	}

	public override void SaveData(TagCompound tag)
	{
		for (int i = 0; i < NumLegacySlots; i++)
		{
			tag.Remove($"CustomAccessory_{i}");
			tag.Remove($"CustomVanity_{i}");
			tag.Remove($"CustomDye_{i}");
		}
	}
}