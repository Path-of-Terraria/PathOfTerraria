using System.Collections.Generic;
using PathOfTerraria.Common.AccessorySlots;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Core.Items;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.Default;

namespace PathOfTerraria.Common.Systems.EquipmentRequirements;

internal enum RequirementAttribute
{
	Strength,
	Dexterity,
	Intelligence,
}

internal enum ArmorRequirementSlot
{
	Helmet,
	Chestplate,
	Leggings,
}

internal readonly record struct ItemRequirements(int Level = 0, int Strength = 0, int Dexterity = 0, int Intelligence = 0)
{
	public static ItemRequirements CreateArmorBase(int minimumDropItemLevel, ArmorRequirementSlot slot, RequirementAttribute attribute)
	{
		int level = Math.Max(1, (minimumDropItemLevel + 1) / 2);
		int attributeValue = minimumDropItemLevel switch
		{
			1 => 0,
			25 => slot switch
			{
				ArmorRequirementSlot.Helmet => 25,
				ArmorRequirementSlot.Chestplate => 30,
				ArmorRequirementSlot.Leggings => 20,
				_ => 0,
			},
			40 => slot switch
			{
				ArmorRequirementSlot.Helmet => 40,
				ArmorRequirementSlot.Chestplate => 45,
				ArmorRequirementSlot.Leggings => 35,
				_ => 0,
			},
			55 => slot switch
			{
				ArmorRequirementSlot.Helmet => 55,
				ArmorRequirementSlot.Chestplate => 60,
				ArmorRequirementSlot.Leggings => 50,
				_ => 0,
			},
			71 => slot switch
			{
				ArmorRequirementSlot.Helmet => 70,
				ArmorRequirementSlot.Chestplate => 80,
				ArmorRequirementSlot.Leggings => 65,
				_ => 0,
			},
			_ => 0,
		};

		return attribute switch
		{
			RequirementAttribute.Strength => new(level, Strength: attributeValue),
			RequirementAttribute.Dexterity => new(level, Dexterity: attributeValue),
			RequirementAttribute.Intelligence => new(level, Intelligence: attributeValue),
			_ => new(level),
		};
	}

	public bool IsMetBy(int level, float strength, float dexterity, float intelligence)
	{
		return level >= Level && strength >= Strength && dexterity >= Dexterity && intelligence >= Intelligence;
	}

	public bool IsMetBy(Player player)
	{
		AttributesPlayer attributes = player.GetModPlayer<AttributesPlayer>();
		return IsMetBy(player.GetModPlayer<ExpModPlayer>().Level, attributes.Strength, attributes.Dexterity, attributes.Intelligence);
	}
}

internal interface IItemRequirements
{
	ItemRequirements Requirements { get; }
}

internal sealed class EquipmentRequirementPlayer : ModPlayer
{
	private readonly HashSet<Item> disabledItems = new(ReferenceEqualityComparer.Instance);

	public static bool IsItemEnabled(Player player, Item item)
	{
		return !player.GetModPlayer<EquipmentRequirementPlayer>().disabledItems.Contains(item);
	}

	public static bool IsEquippedAndDisabled(Player player, Item item)
	{
		return player.active && player.GetModPlayer<EquipmentRequirementPlayer>().disabledItems.Contains(item);
	}

	/// <summary>
	/// Rebuilds the enabled equipment set from non-equipment attributes, then enables requirement-bearing
	/// items one at a time as their requirements are met. This prevents an item from satisfying its own
	/// requirement while still allowing one equipped item to enable another.
	/// </summary>
	internal void RefreshEnabledEquipment()
	{
		disabledItems.Clear();

		List<Item> equipment = GetFunctionalEquipment();
		AttributesPlayer attributes = Player.GetModPlayer<AttributesPlayer>();
		int level = Player.GetModPlayer<ExpModPlayer>().Level;
		float strength = attributes.Strength;
		float dexterity = attributes.Dexterity;
		float intelligence = attributes.Intelligence;
		var pending = new List<(Item Item, ItemRequirements Requirements)>();

		foreach (Item item in equipment)
		{
			if (item.ModItem is IItemRequirements requirementItem)
			{
				pending.Add((item, requirementItem.Requirements));
				disabledItems.Add(item);
				continue;
			}

			AddItemAttributes(item, ref strength, ref dexterity, ref intelligence);
		}

		bool enabledAny;
		do
		{
			enabledAny = false;

			for (int i = pending.Count - 1; i >= 0; i--)
			{
				(Item item, ItemRequirements requirements) = pending[i];
				if (!requirements.IsMetBy(level, strength, dexterity, intelligence))
				{
					continue;
				}

				disabledItems.Remove(item);
				pending.RemoveAt(i);
				AddItemAttributes(item, ref strength, ref dexterity, ref intelligence);
				enabledAny = true;
			}
		}
		while (enabledAny);
	}

	private List<Item> GetFunctionalEquipment()
	{
		var result = new List<Item>();
		var seen = new HashSet<Item>(ReferenceEqualityComparer.Instance);

		int mainItem = Main.mouseItem.IsAir || Main.mouseItem.damage <= 0 ? 0 : 58;
		Add(Player.inventory[mainItem]);

		for (int i = (int)VanillaEquipSlots.Head; i <= (int)VanillaEquipSlots.Accessory7; i++)
		{
			Item item = Player.armor[i];
			if (i == (int)RemappedEquipSlots.Offhand && !AccessorySlotRemapping.IsOffhandCompatible(Player, item))
			{
				continue;
			}

			Add(item);
		}

		AccessorySlotLoader loader = LoaderManager.Get<AccessorySlotLoader>();
		ModAccessorySlotPlayer slotPlayer = Player.GetModPlayer<ModAccessorySlotPlayer>();
		for (int i = 0; i < slotPlayer.SlotCount; i++)
		{
			ModAccessorySlot slot = loader.Get(i, Player);
			if (loader.ModdedIsSpecificItemSlotUnlockedAndUsable(i, Player, false))
			{
				Add(slot.FunctionalItem);
			}
		}

		return result;

		void Add(Item item)
		{
			if (item is { IsAir: false } && seen.Add(item))
			{
				result.Add(item);
			}
		}
	}

	private static void AddItemAttributes(Item item, ref float strength, ref float dexterity, ref float intelligence)
	{
		foreach (ItemAffix affix in item.GetInstanceData().Affixes)
		{
			switch (affix)
			{
				case StrengthItemAffix:
					strength += (int)affix.Value;
					break;
				case DexterityItemAffix:
					dexterity += (int)affix.Value;
					break;
				case IntelligenceItemAffix:
					intelligence += (int)affix.Value;
					break;
			}
		}
	}
}
