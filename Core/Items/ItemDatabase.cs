using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.TreeSystem;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Core.Items;

/// <summary>
///		Keeps track of registered items.
/// </summary>
public sealed class ItemDatabase : ModSystem
{
	private sealed class VanillaItemHandler : GlobalItem
	{
		public override void SetDefaults(Item entity)
		{
			base.SetDefaults(entity);

			if (_vanillaItems.TryGetValue(entity.type, out ItemType itemType))
			{
				entity.GetInstanceData().ItemType = itemType;
			}
		}
	}

	/// <summary>
	///		An item record within the database.
	/// </summary>
	public readonly record struct ItemRecord(float DropChance, ItemRarity Rarity, int ItemId);

	private static List<ItemRecord> _items = [];
	private static Dictionary<int, ItemType> _vanillaItems = [];
	private static HashSet<int> _uniqueVanillaItems = [];

	private const float _magicFindPowerDecrease = 100f;

	public static IReadOnlyCollection<ItemRecord> AllItems => _items;

	public override void PostSetupContent()
	{
		base.PostSetupContent();

		for (int i = 0; i < ItemLoader.ItemCount; i++)
		{
			Item item = ContentSamples.ItemsByType[i];
			PoTStaticItemData staticData = item.GetStaticData();

			if (_vanillaItems.ContainsKey(i))
			{
				staticData.DropChance = 0f;

				if (_uniqueVanillaItems.Contains(i))
				{
					staticData.IsUnique = true;
				}
			}

			// If the drop chance is null (the default value), then this item does not
			// specify a drop chance AT ALL (DIFFERENT from 0%), and should not be
			// registered. This is most prominent in things like vanilla items we
			// haven't intended to every drop.
			if (!staticData.DropChance.HasValue)
			{
				continue;
			}

			float dropChance = staticData.DropChance.Value;

			if (staticData.IsUnique)
			{
				AddItem(dropChance, ItemRarity.Unique, i);
			}
			else
			{
				// TODO: Hardcoded :(
				if (item.ModItem is not Jewel)
				{
					AddItem(dropChance * 0.7f, ItemRarity.Normal, i);
				}

				AddItem(dropChance * 0.25f, ItemRarity.Magic, i);
				AddItem(dropChance * 0.05f, ItemRarity.Rare, i);
			}
		}
	}

	public override void Unload()
	{
		base.Unload();

		_items = null;
		_vanillaItems = null;
		_uniqueVanillaItems = null;
	}

	public static void AddItem(float dropChance, ItemRarity rarity, int itemId)
	{
		_items.Add(new ItemRecord(dropChance, rarity, itemId));
	}

	public static void AddItemWithVariableRarity(float dropChance, int itemId)
	{
		AddItem(dropChance * 0.7f, ItemRarity.Normal, itemId);
		AddItem(dropChance * 0.25f, ItemRarity.Magic, itemId);
		AddItem(dropChance * 0.05f, ItemRarity.Rare, itemId);
	}

	public static float ApplyRarityModifier(float chance, float dropRarityModifier)
	{
		// this is just some arbitrary function from chat gpt, modified a little...
		// it is pretty hard to get all this down when we dont know all the items we will have n such;

		chance *= 100f; // to make it effective on <0.1; it works... ok?
		float powerDecrease = chance * (1 + dropRarityModifier / _magicFindPowerDecrease) /
							  (1 + chance * dropRarityModifier / _magicFindPowerDecrease);
		return powerDecrease;
	}

	public static void RegisterVanillaItemAsGear(int itemId, ItemType itemType)
	{
		GearGlobalItem.MarkItemAsGear(itemId);
		_vanillaItems[itemId] = itemType;
	}

	public static void RegisterUniqueVanillaItemAsGear(int itemId, ItemType itemType)
	{
		RegisterVanillaItemAsGear(itemId, itemType);
		_uniqueVanillaItems.Add(itemId);
	}
}
