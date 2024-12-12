using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Content.Items.Consumables.Maps;
using PathOfTerraria.Content.Items.Currency;
using PathOfTerraria.Content.Items.Gear;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common;

internal class ItemSpawner
{
	/// <summary>
	/// Starts the drop process for a MobKill
	/// </summary>
	/// <param name="pos"></param>
	/// <param name="iLevel"></param>
	/// <param name="dropRarityModifier"></param>
	public static void SpawnMobKillItem(Vector2 pos, int iLevel = 0, float dropRarityModifier = 0)
	{
		int rand = Main.rand.Next(1, 100);
		switch (rand)
		{
			case <= 3: //3% Map
				SpawnRandomItemByType<Map>(pos, iLevel, dropRarityModifier);
				break;
			case <= 10: //7% Currency
				SpawnRandomItemByType<CurrencyShard>(pos, iLevel, dropRarityModifier);
				break;
			default: //90% Gear
				SpawnRandomItemByType<Gear>(pos, iLevel, dropRarityModifier);
				break;
		}
	}
	
	/// <summary>
	/// Spawns a random item, based on the item level and drop rarity modifier.
	/// </summary>
	/// <param name="pos"></param>
	/// <param name="iLevel"></param>
	/// <param name="dropRarityModifier"></param>
	public static void SpawnRandomItem(Vector2 pos, int iLevel = 0, float dropRarityModifier = 0)
	{
		SpawnRandomItem(pos, x => true, iLevel, dropRarityModifier);
	}
	
	/// <summary>
	///    Spawns a random item by it's type
	/// </summary>
	public static void SpawnRandomItemByType<T>(Vector2 pos, int iLevel, float dropRarityModifier = 0)
	{
		IEnumerable<ItemDatabase.ItemRecord> items = ItemDatabase.GetItemByType<T>();
		SpawnItemFromList(pos, x => true, iLevel, dropRarityModifier, items.ToList());
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="pos"></param>
	/// <param name="dropCondition"></param>
	/// <param name="iLevel"></param>
	/// <param name="dropRarityModifier"></param>
	/// <returns></returns>
	public static int SpawnRandomItem(Vector2 pos, Func<ItemDatabase.ItemRecord, bool> dropCondition,
		int iLevel = 0, float dropRarityModifier = 0)
	{
		iLevel = iLevel == 0 ? PoTItemHelper.PickItemLevel() : iLevel; // Pick the item level if not provided

		// Filter AllGear based on item level
		var filteredGear = ItemDatabase.AllItems.Where(g => 
		{
			PoTStaticItemData staticData = ContentSamples.ItemsByType[g.ItemId].GetStaticData();
			return staticData.MinDropItemLevel <= iLevel;
		}).ToList();

		return SpawnItemFromList(pos, dropCondition, iLevel, dropRarityModifier, filteredGear);
	}
	
	/// <summary>
	/// 
	/// </summary>
	/// <param name="pos"></param>
	/// <param name="dropCondition"></param>
	/// <param name="iLevel"></param>
	/// <param name="dropRarityModifier"></param>
	/// <param name="filteredGear"></param>
	/// <returns></returns>
	private static int SpawnItemFromList(Vector2 pos, Func<ItemDatabase.ItemRecord, bool> dropCondition, int iLevel,
		float dropRarityModifier, List<ItemDatabase.ItemRecord> filteredGear)
	{
		dropRarityModifier += iLevel / 10f; // the effect of item level on "magic find"

		// Calculate dropChanceSum based on filtered gear
		float dropChanceSum = filteredGear.Where(dropCondition).Sum(x =>
			ItemDatabase.ApplyRarityModifier(x.DropChance, dropRarityModifier));
		float choice = Main.rand.NextFloat(dropChanceSum);

		float cumulativeChance = 0;

		foreach (ItemDatabase.ItemRecord item in filteredGear)
		{
			if (!dropCondition(item))
			{
				continue;
			}

			cumulativeChance += ItemDatabase.ApplyRarityModifier(item.DropChance, dropRarityModifier);
			if (choice < cumulativeChance)
			{
				// Spawn the item
				return SpawnItem(item.ItemId, pos, iLevel, item.Rarity);
			}
		}

		return -1;
	}

	/// <summary>
	/// Spawns a random piece of gear of the given type at the given position.
	/// </summary>
	/// <typeparam name="T">The type of gear to drop</typeparam>
	/// <param name="pos">Where to drop it in the world</param>
	/// <param name="itemLevel">The item level of the item to spawn</param>
	/// <param name="rarity">Rarity of the item</param>
	public static int SpawnItem<T>(Vector2 pos, int itemLevel = 0, ItemRarity rarity = ItemRarity.Normal) where T : ModItem
	{
		return SpawnItem(ModContent.ItemType<T>(), pos, itemLevel, rarity);
	}

	/// <summary>
	/// Spawns a random piece of gear from the given superclass (i.e. <see cref="Content.Items.Gear.Weapons.Sword.Sword"/>) at the given position.
	/// </summary>
	/// <typeparam name="T">The type of gear to drop</typeparam>
	/// <param name="pos">Where to drop it in the world</param>
	/// <param name="itemLevel">The item level of the item to spawn</param>
	/// <param name="rarity">Rarity of the item</param>
	public static int SpawnItemFromCategory<T>(Vector2 pos, int itemLevel = 0, ItemRarity rarity = ItemRarity.Normal) where T : ModItem
	{
		return SpawnItem(Main.rand.Next(ModContent.GetContent<T>().ToArray()).Type, pos, itemLevel, rarity);
	}

	/// <summary>
	/// Spawns a random piece of gear of the given base type at the given position
	/// </summary>
	/// <param name="type"></param>
	/// <param name="pos">Where to drop it in the world</param>
	/// <param name="itemLevel">The item level of the item to spawn</param>
	/// <param name="rarity">Rarity of the item</param>
	public static int SpawnItem(int type, Vector2 pos, int itemLevel = 0, ItemRarity rarity = ItemRarity.Normal)
	{
		var item = new Item(type);
		PoTInstanceItemData data = item.GetInstanceData();
		PoTStaticItemData staticData = item.GetStaticData();

		if (staticData.IsUnique)
		{
			rarity = ItemRarity.Unique;
		}

		data.Rarity = rarity;
		PoTItemHelper.Roll(item, itemLevel == 0 ? PoTItemHelper.PickItemLevel() : itemLevel);

		return Main.netMode switch
		{
			NetmodeID.SinglePlayer => Item.NewItem(null, pos, Vector2.Zero, item),
			NetmodeID.MultiplayerClient => Main.LocalPlayer.QuickSpawnItem(new EntitySource_DebugCommand("/spawnitem"),
				item),
			_ => -1
		};
	}
}
