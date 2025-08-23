using Terraria.ModLoader.Default;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.AccessorySlots;

[Autoload(false)]
public sealed class ExtraAccessorySlot : ModAccessorySlot
{
	public required int IndexInMod { get; init; }

	public override string Name => $"{GetType().Name}" + IndexInMod;

	public override bool IsEnabled()
	{
		return ExtraAccessorySlots.IsLocalSlotActive(IndexInMod);
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
		return true;
	}

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

	public static ExtraAccessorySlot GetByLocalIndex(int localIndex)
	{
		return slots[localIndex];
	}

	public static bool IsLocalSlotActive(int localIndex)
	{
		return localIndex switch
		{
			0 => true, // Add one slot by default.
			1 => Main.hardMode, // Add one slot after WoF is defeated.
			_ => false,
		};
	}
}