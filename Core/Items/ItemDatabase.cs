using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.TreeSystem;
using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Content.Items.Currency;
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

	private const float MagicDropChance = 0.85f;
	private const float RareDropChance = 0.15f;

	/// <summary>
	///		An item record within the database.
	/// </summary>
	public readonly record struct ItemRecord(float DropChance, ItemRarity Rarity, int ItemId, Item item);

	private static List<ItemRecord> _items = [];
	private static Dictionary<int, ItemType> _vanillaItems = [];
	private static HashSet<int> _uniqueVanillaItems = [];

	private const float MagicFindPowerDecrease = 100f;

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
				AddItem(dropChance, ItemRarity.Unique, i, item);
			}
			else
			{
				AddItem(dropChance * MagicDropChance, ItemRarity.Magic, i, item);
				AddItem(dropChance * RareDropChance, ItemRarity.Rare, i, item);
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

	public static void AddItem(float dropChance, ItemRarity rarity, int itemId, Item item)
	{
		_items.Add(new ItemRecord(dropChance, rarity, itemId, item));
	}

	public static float ApplyRarityModifier(float chance, float dropRarityModifier)
	{
		// this is just some arbitrary function from chat gpt, modified a little...
		// it is pretty hard to get all this down when we dont know all the items we will have n such;

		chance *= 100f; // to make it effective on <0.1; it works... ok?
		float powerDecrease = chance * (1 + dropRarityModifier / MagicFindPowerDecrease) /
							  (1 + chance * dropRarityModifier / MagicFindPowerDecrease);
		return powerDecrease;
	}
	
	/// <summary>
	/// Gets all the matching items from the database.
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<ItemRecord> GetItemByType<T>()
	{
		return _items.Where(x => x.item.ModItem is T);
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
