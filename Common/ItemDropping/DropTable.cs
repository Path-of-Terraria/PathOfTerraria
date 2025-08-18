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
		UnifiedRandom random = null, ItemRarity forceRarity = ItemRarity.Invalid)
	{
		var chances = new WeightedRandom<int>(random ?? Main.rand);
		chances.Add(0, gearChance);
		chances.Add(1, currencyChance);
		chances.Add(2, mapChance);

		int choice = chances.Get();
		IEnumerable<ItemDatabase.ItemRecord> items = choice switch
		{
			//Gear
			0 => ItemDatabase.GetItemByType<Gear>(),
			//Currency
			1 => ItemDatabase.GetItemByType<CurrencyShard>(),
			//Maps
			_ => GetMapPool(random),
		};

		if (forceRarity != ItemRarity.Invalid)
		{
			items = items.Where(x => x.Rarity == forceRarity);
		}

		return RollList(itemLevel, dropRarityModifier, [.. items]);
	}

	/// <summary>
	/// Drops many items based off of the following table: <br/>
	/// <c>Gear:</c> Default 80%<br/>
	/// <c>Currency:</c> Default 15%<br/>
	/// <c>Maps:</c> Default 5%<br/>
	/// </summary>
	/// <param name="count">Amount of items to drop.</param>
	/// <param name="itemLevel">Level to drop them at.</param>
	/// <param name="dropRarityModifier">
	/// The rarity modifier of the drops. 100 = 100% increase for both Rare and Unique drops, taken out of Magic's pool.<br/>
	/// For exampe, if we have Magic at 90%, Rare at 7% and Unique at 3%, 100% <paramref name="dropRarityModifier"/> would turn it into Magic 80%, Rare 14% and Unique 6%.
	/// </param>
	/// <param name="gearChance">Drop rate for <see cref="Gear"/>. Defaults to 80%.</param>
	/// <param name="currencyChance">Drop rate for <see cref="CurrencyShard"/>s. Defaults to 15%.</param>
	/// <param name="mapChance">
	/// Drop rate for <see cref="Map"/>s. Defaults to 5%.<br/>
	/// Note that in <see cref="Main.hardMode"/>, this is split into 70% Explorable maps, and 30% boss maps.
	/// </param>
	/// <param name="random">The <see cref="UnifiedRandom"/> to use for randomization. Use <see cref="WorldGen.genRand"/> for generation, such as placing items in chests.</param>
	/// <param name="forceRarity">The rarity that <b>must</b> drop from this, if any. Defaults to <see cref="ItemRarity.Invalid"/>, which will allow any rarity.</param>
	/// <returns></returns>
	public static List<ItemDatabase.ItemRecord> RollManyMobDrops(int count, int itemLevel, float dropRarityModifier, float gearChance = 0.8f, float currencyChance = 0.15f, 
		float mapChance = 0.05f, UnifiedRandom random = null, ItemRarity forceRarity = ItemRarity.Invalid, float itemRarityModifier = 0)
	{
		random ??= Main.rand;
		var chances = new WeightedRandom<WeightedRandom<ItemDatabase.ItemRecord>>(random);
		chances.Add(GetGearPool(itemLevel, ref dropRarityModifier, [.. ItemDatabase.GetItemByType<Gear>()], IsRecordValid, itemRarityModifier), gearChance);
		chances.Add(GetGearPool(itemLevel, ref dropRarityModifier, [.. ItemDatabase.GetItemByType<CurrencyShard>()], IsRecordValid, itemRarityModifier), currencyChance);
		chances.Add(GetWeightedMapPool(random), mapChance);

		List<ItemDatabase.ItemRecord> items = [];

		for (int i = 0; i < count; ++i)
		{
			items.Add(chances.Get().Get());
		}

		return items;

		bool IsRecordValid(ItemDatabase.ItemRecord record)
		{
			return forceRarity == ItemRarity.Invalid || record.Rarity == forceRarity;
		}
	}

	private static IEnumerable<ItemDatabase.ItemRecord> GetMapPool(UnifiedRandom random)
	{
		IEnumerable<ItemDatabase.ItemRecord> items;

		if (Main.hardMode)
		{
			IEnumerable<ItemDatabase.ItemRecord> allMaps = ItemDatabase.GetItemByType<Map>().Where(x => ((Map)x.Item.ModItem).CanDrop);
			IEnumerable<ItemDatabase.ItemRecord> itemRecords = allMaps as ItemDatabase.ItemRecord[] ?? [.. allMaps];
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

		return items;
	}

	private static WeightedRandom<ItemDatabase.ItemRecord> GetWeightedMapPool(UnifiedRandom random)
	{
		WeightedRandom<ItemDatabase.ItemRecord> items = new(random);

		if (Main.hardMode)
		{
			IEnumerable<ItemDatabase.ItemRecord> allMaps = ItemDatabase.GetItemByType<Map>().Where(x => ((Map)x.Item.ModItem).CanDrop);
			IEnumerable<ItemDatabase.ItemRecord> itemRecords = allMaps as ItemDatabase.ItemRecord[] ?? [.. allMaps];
			IEnumerable<ItemDatabase.ItemRecord> explorableMaps = itemRecords.Where(x => x.Item.ModItem is Content.Items.Consumables.Maps.ExplorableMaps.ExplorableMap);
			IEnumerable<ItemDatabase.ItemRecord> bossMaps = itemRecords.Where(x => x.Item.ModItem is not Content.Items.Consumables.Maps.ExplorableMaps.ExplorableMap);

			foreach (ItemDatabase.ItemRecord record in explorableMaps)
			{
				items.Add(record, 0.7f);
			}

			foreach (ItemDatabase.ItemRecord record in bossMaps)
			{
				items.Add(record, 0.3f);
			}
		}
		else
		{
			IEnumerable<ItemDatabase.ItemRecord> list = ItemDatabase.GetItemByType<Map>().Where(x => (x.Item.ModItem as Map).CanDrop);

			foreach (ItemDatabase.ItemRecord record in list)
			{
				items.Add(record);
			}
		}

		return items;
	}

	private static ItemDatabase.ItemRecord RollList(int itemLevel, float dropRarityModifier, List<ItemDatabase.ItemRecord> filteredGear)
	{
		return RollList(itemLevel, dropRarityModifier, filteredGear, _ => true, 0);
	}

	public static ItemDatabase.ItemRecord RollList(int itemLevel, float dropRarityModifier, List<ItemDatabase.ItemRecord> filteredGear, 
		Func<ItemDatabase.ItemRecord, bool> additionalCondition, float itemRarityModifier)
	{
		WeightedRandom<ItemDatabase.ItemRecord> selection = GetGearPool(itemLevel, ref dropRarityModifier, filteredGear, additionalCondition, itemRarityModifier);
		return selection.elements.Count == 0 ? ItemDatabase.InvalidItem : selection.Get();
	}

	private static WeightedRandom<ItemDatabase.ItemRecord> GetGearPool(int itemLevel, ref float dropRarityModifier, List<ItemDatabase.ItemRecord> filteredGear, 
		Func<ItemDatabase.ItemRecord, bool> additionalCondition, float itemRarityModifier)
	{
		dropRarityModifier += itemLevel / 10f; // Higher levels have a higher likelihood of being Rare or Unique

		float rarityMod = dropRarityModifier;
		float dropChanceSum = filteredGear.Where(additionalCondition).Sum(x => ItemDatabase.ApplyDropRateModifiers(x, rarityMod, itemRarityModifier));
		float choice = Main.rand.NextFloat(dropChanceSum);
		WeightedRandom<ItemDatabase.ItemRecord> selection = new();

		foreach (ItemDatabase.ItemRecord item in filteredGear)
		{
			selection.Add(item, ItemDatabase.ApplyDropRateModifiers(item, rarityMod, itemRarityModifier));
		}

		return selection;
	}
}
