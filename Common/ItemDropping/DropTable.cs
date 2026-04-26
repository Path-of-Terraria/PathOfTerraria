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
	/// <param name="npc">The NPC being killed. When provided, map drop weights are adjusted based on biome/time context.</param>
	public static ItemDatabase.ItemRecord RollMobDrops(int itemLevel, float dropRarityModifier, float gearChance = 0.8f, float currencyChance = 0.15f, float mapChance = 0.05f, 
		UnifiedRandom random = null, ItemRarity forceRarity = ItemRarity.Invalid, float uniqueModifier = 1f, NPC npc = null)
	{
		random ??= Main.rand;
		var chances = new WeightedRandom<int>(random);
		chances.Add(0, gearChance);
		chances.Add(1, currencyChance);
		chances.Add(2, mapChance);

		int choice = chances.Get();

		if (choice == 2)
		{
			WeightedRandom<ItemDatabase.ItemRecord> mapPool = GetWeightedMapPool(random, npc);
			if (forceRarity != ItemRarity.Invalid)
			{
				WeightedRandom<ItemDatabase.ItemRecord> filtered = new(random);
				foreach (var element in mapPool.elements)
				{
					if (element.Item1.Rarity == forceRarity)
					{
						filtered.Add(element.Item1, (float)element.Item2);
					}
				}
				mapPool = filtered;
			}
			return mapPool.elements.Count == 0 ? ItemDatabase.InvalidItem : mapPool.Get();
		}

		IEnumerable<ItemDatabase.ItemRecord> items = choice switch
		{
			//Gear
			0 => ItemDatabase.GetItemByType<Gear>().Where(x => MeetsMinDropItemLevel(x, itemLevel)),
			//Currency
			_ => ItemDatabase.GetItemByType<CurrencyShard>().Where(x => MeetsMinDropItemLevel(x, itemLevel)),
		};

		if (forceRarity != ItemRarity.Invalid)
		{
			items = items.Where(x => x.Rarity == forceRarity);
		}

		return RollList(itemLevel, dropRarityModifier, [.. items], x => true, 0f, null, uniqueModifier);
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
	/// <param name="npc">The NPC being killed. When provided, map drop weights are adjusted based on biome/time context.</param>
	/// <returns></returns>
	public static List<ItemDatabase.ItemRecord> RollManyMobDrops(int count, int itemLevel, float dropRarityModifier, float gearChance = 0.8f, float currencyChance = 0.15f, 
		float mapChance = 0.05f, UnifiedRandom random = null, ItemRarity forceRarity = ItemRarity.Invalid, float itemRarityModifier = 0, NPC npc = null)
	{
		random ??= Main.rand;
		var chances = new WeightedRandom<WeightedRandom<ItemDatabase.ItemRecord>>(random);
		chances.Add(GetGearPool(itemLevel, ref dropRarityModifier, [.. ItemDatabase.GetItemByType<Gear>()], IsRecordValid, itemRarityModifier, random), gearChance);
		chances.Add(GetGearPool(itemLevel, ref dropRarityModifier, [.. ItemDatabase.GetItemByType<CurrencyShard>()], IsRecordValid, itemRarityModifier, random), currencyChance);
		chances.Add(GetWeightedMapPool(random, npc), mapChance);

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

	private static WeightedRandom<ItemDatabase.ItemRecord> GetWeightedMapPool(UnifiedRandom random, NPC npc = null)
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
				items.Add(record, 0.7f * GetMapDropWeight(record, npc));
			}

			foreach (ItemDatabase.ItemRecord record in bossMaps)
			{
				items.Add(record, 0.3f * GetMapDropWeight(record, npc));
			}
		}
		else
		{
			IEnumerable<ItemDatabase.ItemRecord> list = ItemDatabase.GetItemByType<Map>().Where(x => (x.Item.ModItem as Map).CanDrop);

			foreach (ItemDatabase.ItemRecord record in list)
			{
				items.Add(record, GetMapDropWeight(record, npc));
			}
		}

		return items;
	}

	/// <summary>
	/// Returns the context-based drop weight for a map record, determined by the NPC being killed.
	/// When <paramref name="npc"/> is <see langword="null"/> (e.g. chest loot), returns 1 (no adjustment).
	/// </summary>
	private static float GetMapDropWeight(ItemDatabase.ItemRecord record, NPC npc)
	{
		return npc != null ? ((Map)record.Item.ModItem).GetDropWeight(npc) : 1f;
	}

	private static ItemDatabase.ItemRecord RollList(int itemLevel, float rarityMod, List<ItemDatabase.ItemRecord> filteredGear, UnifiedRandom random = null, float uniqueModifier = 1f)
	{
		return RollList(itemLevel, rarityMod, filteredGear, _ => true, 0, random, uniqueModifier);
	}

	public static ItemDatabase.ItemRecord RollList(int itemLevel, float dropRarityModifier, List<ItemDatabase.ItemRecord> filteredGear, 
		Func<ItemDatabase.ItemRecord, bool> additionalCondition, float itemRarityModifier, UnifiedRandom random = null, float uniqueModifier = 1f)
	{
		WeightedRandom<ItemDatabase.ItemRecord> selection = GetGearPool(itemLevel, ref dropRarityModifier, filteredGear, additionalCondition, itemRarityModifier, random, uniqueModifier);
		return selection.elements.Count == 0 ? ItemDatabase.InvalidItem : selection.Get();
	}

	private static WeightedRandom<ItemDatabase.ItemRecord> GetGearPool(int itemLevel, ref float dropRarityModifier, List<ItemDatabase.ItemRecord> filteredGear, 
		Func<ItemDatabase.ItemRecord, bool> additionalCondition, float itemRarityModifier, UnifiedRandom random = null, float uniqueModifier = 1f)
	{
		random ??= Main.rand;
		dropRarityModifier += itemLevel / 10f; // Higher levels have a higher likelihood of being Magic, Rare or Unique
		WeightedRandom<ItemDatabase.ItemRecord> selection = new(random);

		foreach (ItemDatabase.ItemRecord item in filteredGear)
		{
			if (MeetsMinDropItemLevel(item, itemLevel) && additionalCondition.Invoke(item))
			{
				selection.Add(item, ItemDatabase.ApplyDropRateModifiers(item, dropRarityModifier, itemRarityModifier, uniqueModifier));
			}
		}

		return selection;
	}

	private static bool MeetsMinDropItemLevel(ItemDatabase.ItemRecord record, int itemLevel)
	{
		PoTStaticItemData staticData = record.Item.GetStaticData();
		return staticData.MinDropItemLevel <= itemLevel;
	}
}
