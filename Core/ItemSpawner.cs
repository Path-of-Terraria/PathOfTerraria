using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Core;

internal class ItemSpawner
{
	public static void SpawnRandomItem(Vector2 pos, int ilevel = 0, float dropRarityModifier = 0)
	{
		SpawnRandomItem(pos, x => true, ilevel, dropRarityModifier);
	}

	public static int SpawnRandomItem(Vector2 pos, Func<Tuple<float, Rarity, Type>, bool> dropCondition,
		int ilevel = 0, float dropRarityModifier = 0)
	{
		ilevel = ilevel == 0 ? PoTItem.PickItemLevel() : ilevel; // Pick the item level if not provided

		// Filter AllGear based on item level
		var filteredGear = PoTItem.AllItems.Where(g =>
		{
			var gearInstance = Activator.CreateInstance(g.Item3) as PoTItem;
			return gearInstance != null && gearInstance.MinDropItemLevel <= ilevel;
		}).ToList();

		return SpawnItemFromList(pos, dropCondition, ilevel, dropRarityModifier, filteredGear);
	}

	private static int SpawnItemFromList(Vector2 pos, Func<Tuple<float, Rarity, Type>, bool> dropCondition, int ilevel,
		float dropRarityModifier, List<Tuple<float, Rarity, Type>> filteredGear)
	{
		dropRarityModifier += ilevel / 10f; // the effect of item level on "magic find"

		// Calculate dropChanceSum based on filtered gear
		float dropChanceSum = filteredGear.Where(x => dropCondition(x)).Sum((Tuple<float, Rarity, Type> x) =>
			PoTItem.ApplyRarityModifier(x.Item1, dropRarityModifier));
		float choice = Main.rand.NextFloat(dropChanceSum);

		float cumulativeChance = 0;

		foreach (Tuple<float, Rarity, Type> item in filteredGear)
		{
			if (!dropCondition(item))
			{
				continue;
			}

			cumulativeChance += PoTItem.ApplyRarityModifier(item.Item1, dropRarityModifier);
			if (choice < cumulativeChance)
			{
				// Spawn the item
				string itemName = (Activator.CreateInstance(item.Item3) as ModItem).Name;
				int itemType = ModLoader.GetMod("PathOfTerraria").Find<ModItem>(itemName).Type;
				return SpawnItem(itemType, pos, ilevel, item.Item2); ;
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
	public static int SpawnItem<T>(Vector2 pos, int itemLevel = 0, Rarity rarity = Rarity.Normal) where T : PoTItem
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
	public static int SpawnItemFromCategory<T>(Vector2 pos, int itemLevel = 0, Rarity rarity = Rarity.Normal) where T : PoTItem
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
	public static int SpawnItem(int type, Vector2 pos, int itemLevel = 0, Rarity rarity = Rarity.Normal)
	{
		var item = new Item(type);
		var gear = item.ModItem as PoTItem;

		if (gear.IsUnique)
		{
			rarity = Rarity.Unique;
		}

		gear.Rarity = rarity;
		gear.Roll(itemLevel == 0 ? PoTItem.PickItemLevel() : itemLevel);

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
