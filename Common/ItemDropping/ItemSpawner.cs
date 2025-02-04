using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Enums;
using Terraria.DataStructures;
using Terraria.ID;
using PathOfTerraria.Content.Items.Consumables.Maps;

namespace PathOfTerraria.Common.ItemDropping;

internal class ItemSpawner
{
	/// <summary>
	/// Drops an item based off of the following table:<br/>
	/// <c>Gear:</c> Default 8%<br/>
	/// <c>Currency:</c> Default 15%<br/>
	/// <c>Maps:</c> Default 5%<br/>
	/// </summary>
	/// <param name="pos">Position to spawn the item on.</param>
	/// <param name="itemLevel">Level of the item spawned. Defaults to 0, which rolls at the current world level.</param>
	/// <param name="dropRarityModifier">Drop modifier. Higher = more likely to get rare items.</param>
	public static int SpawnMobKillItem(Vector2 pos, int itemLevel = 0, float dropRarityModifier = 0, float gearChance = 0.8f, float curChance = 0.15f, float mapChance = 0.05f)
	{
		ItemDatabase.ItemRecord item = DropTable.RollMobDrops(itemLevel, dropRarityModifier, gearChance, curChance, mapChance);

		if (item == ItemDatabase.InvalidItem)
		{
			return -1;
		}

		return SpawnItem(item.ItemId, pos, itemLevel, item.Rarity);
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
	/// Shorthand for calling <see cref="DropTable.RollList(int, float, List{ItemDatabase.ItemRecord}, Func{ItemDatabase.ItemRecord, bool})"/> and spawning an item with it.
	/// </summary>
	/// <param name="pos">Position of the spawned item.</param>
	/// <param name="dropCondition">Additional condition for spawning, if any.</param>
	/// <param name="itemLevel">Item level.</param>
	/// <param name="dropRarityModifier">Modifier for the rarity of drops.</param>
	/// <param name="filteredGear">List of pre-filtered gear.</param>
	/// <returns>The index of the newly spawned item in Main.item.</returns>
	private static int SpawnItemFromList(Vector2 pos, Func<ItemDatabase.ItemRecord, bool> dropCondition, int itemLevel,
		float dropRarityModifier, List<ItemDatabase.ItemRecord> filteredGear)
	{
		ItemDatabase.ItemRecord item = DropTable.RollList(itemLevel, dropRarityModifier, filteredGear, dropCondition);

		if (item == ItemDatabase.InvalidItem)
		{
			return -1;
		}

		return SpawnItem(item.ItemId, pos, itemLevel, item.Rarity);
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
			NetmodeID.SinglePlayer => Item.NewItem(new EntitySource_DebugCommand("/spawnitem"), pos, Vector2.Zero, item),
			NetmodeID.MultiplayerClient => Main.LocalPlayer.QuickSpawnItem(new EntitySource_DebugCommand("/spawnitem"),
				item),
			_ => -1
		};
	}

	/// <summary>
	/// Used by the SpawnMap command. This seems useless, but I've ported it if only for posterity.
	/// </summary>
	/// <param name="pos"></param>
	public static void SpawnMap<T>(Vector2 pos, int tier) where T : Map
	{
		var item = new Item();
		item.SetDefaults(ModContent.ItemType<T>());
		(item.ModItem as Map).Tier = tier;

		Item.NewItem(null, pos, Vector2.Zero, item);
	}
}
