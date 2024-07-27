using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Enums;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common;

internal class ItemSpawner
{
	public static void SpawnRandomItem(Vector2 pos, int ilevel = 0, float dropRarityModifier = 0)
	{
		SpawnRandomItem(pos, x => true, ilevel, dropRarityModifier);
	}

	public static int SpawnRandomItem(Vector2 pos, Func<ItemDatabase.ItemRecord, bool> dropCondition,
		int ilevel = 0, float dropRarityModifier = 0)
	{
		ilevel = ilevel == 0 ? PoTItemHelper.PickItemLevel() : ilevel; // Pick the item level if not provided

		// Filter AllGear based on item level
		var filteredGear = ItemDatabase.AllItems.Where(g => ContentSamples.ItemsByType[g.ItemId].GetStaticData().MinDropItemLevel <= ilevel).ToList();

		return SpawnItemFromList(pos, dropCondition, ilevel, dropRarityModifier, filteredGear);
	}


	private static int SpawnItemFromList(Vector2 pos, Func<ItemDatabase.ItemRecord, bool> dropCondition, int ilevel,
		float dropRarityModifier, List<ItemDatabase.ItemRecord> filteredGear)
	{
		dropRarityModifier += ilevel / 10f; // the effect of item level on "magic find"

		// Calculate dropChanceSum based on filtered gear
		float dropChanceSum = filteredGear.Where(x => dropCondition(x)).Sum((ItemDatabase.ItemRecord x) =>
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
				return SpawnItem(item.ItemId, pos, ilevel, item.Rarity);
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
	/// <param name="dropRarityModifier">Rolls an item with a drop rarity modifier</param>
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
	/// <param name="dropRarityModifier">Rolls an item with a drop rarity modifier</param>
	public static int SpawnItemFromCategory<T>(Vector2 pos, int itemLevel = 0, ItemRarity rarity = ItemRarity.Normal) where T : ModItem
	{
		return SpawnItem(Main.rand.Next(ModContent.GetContent<T>().ToArray()).Type, pos, itemLevel, rarity);
	}

	/// <summary>
	/// Spawns a random piece of gear of the given base type at the given position
	/// </summary>
	/// <typeparam name="T">The type of gear to drop</typeparam>
	/// <param name="pos">Where to drop it in the world</param>
	/// <param name="itemLevel">The item level of the item to spawn</param>
	/// <param name="dropRarityModifier">Rolls an item with a drop rarity modifier</param>
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

		if (Main.netMode == NetmodeID.SinglePlayer)
		{
			return Item.NewItem(null, pos, Vector2.Zero, item);
		}
		else if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			return Main.LocalPlayer.QuickSpawnItem(new EntitySource_DebugCommand("/spawnitem"), item);
		}

		return -1;
	}
}
