using PathOfTerraria.Common.Enums;
using PathOfTerraria.Content.Items.Consumables.Maps;
using PathOfTerraria.Content.Items.Currency;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using System.Linq;
using Terraria.Utilities;

namespace PathOfTerraria.Common.ItemDropping;

internal class DropTable
{
	/// <summary>
	/// Drops an item based off of the following table:<br/>
	/// <c>Gear:</c> Default 80%<br/>
	/// <c>Currency:</c> Default 15%<br/>
	/// <c>Maps:</c> Default 5%<br/>
	/// </summary>
	/// <param name="random">The random to 'seed' the choice with. Defaults to Main.rand.</param>
	public static ItemDatabase.ItemRecord RollMobDrops(int itemLevel, float dropRarityModifier, float gearChance = 0.8f, float currencyChance = 0.15f, float mapChance = 0.05f, 
		UnifiedRandom random = null, ItemRarity forceRarity = (ItemRarity)(-1))
	{
		var chances = new WeightedRandom<int>(random ?? Main.rand);
		chances.Add(0, gearChance);
		chances.Add(1, currencyChance);
		chances.Add(2, mapChance);

		int choice = chances.Get();
		IEnumerable<ItemDatabase.ItemRecord> items;

		switch (choice)
		{
			case 0: //Gear
				items = ItemDatabase.GetItemByType<Gear>();
				break;
			case 1: //Currency
				items = ItemDatabase.GetItemByType<CurrencyShard>();
				break;
			default: //Maps
				{
					if (Main.hardMode)
					{
						IEnumerable<ItemDatabase.ItemRecord> allMaps = ItemDatabase.GetItemByType<Map>().Where(x => ((Map)x.Item.ModItem).CanDrop);
						IEnumerable<ItemDatabase.ItemRecord> itemRecords = allMaps as ItemDatabase.ItemRecord[] ?? allMaps.ToArray();
						IEnumerable<ItemDatabase.ItemRecord> explorableMaps = itemRecords.Where(x => x.Item.ModItem is Content.Items.Consumables.Maps.ExplorableMaps.ExplorableMap);
						IEnumerable<ItemDatabase.ItemRecord> bossMaps = itemRecords.Where(x => x.Item.ModItem is not Content.Items.Consumables.Maps.ExplorableMaps.ExplorableMap);

						var mapTypeChances = new WeightedRandom<int>(random ?? Main.rand);
						mapTypeChances.Add(0, 0.7f); //70% explorable maps
						mapTypeChances.Add(1, 0.3f); //30% boss domain maps
						int mapTypeChoice = mapTypeChances.Get();

						if (mapTypeChoice == 0)
						{
							items = explorableMaps;
						}
						else
						{
							items = bossMaps;
						}
					}
					else
					{
						items = ItemDatabase.GetItemByType<Map>().Where(x => (x.Item.ModItem as Map).CanDrop);
					}
					
					break;
				}
		}

		if (forceRarity != (ItemRarity)(-1))
		{
			items = items.Where(x => x.Rarity == forceRarity);
		}

		return RollList(itemLevel, dropRarityModifier, items.ToList());
	}

	private static ItemDatabase.ItemRecord RollList(int itemLevel, float dropRarityModifier, List<ItemDatabase.ItemRecord> filteredGear)
	{
		return RollList(itemLevel, dropRarityModifier, filteredGear, _ => true);
	}

	public static ItemDatabase.ItemRecord RollList(int itemLevel, float dropRarityModifier, List<ItemDatabase.ItemRecord> filteredGear, 
		Func<ItemDatabase.ItemRecord, bool> additionalCondition)
	{
		dropRarityModifier += itemLevel / 10f; // the effect of item level on "magic find"

		// Calculate dropChanceSum based on filtered gear
		float dropChanceSum = filteredGear.Where(additionalCondition).Sum(x => ItemDatabase.ApplyRarityModifier(x.DropChance, dropRarityModifier));
		float choice = Main.rand.NextFloat(dropChanceSum);

		float cumulativeChance = 0;

		foreach (ItemDatabase.ItemRecord item in filteredGear)
		{
			if (!additionalCondition(item))
			{
				continue;
			}

			cumulativeChance += ItemDatabase.ApplyRarityModifier(item.DropChance, dropRarityModifier);

			if (choice < cumulativeChance)
			{
				return item;
			}
		}

		return ItemDatabase.InvalidItem;
	}
}
