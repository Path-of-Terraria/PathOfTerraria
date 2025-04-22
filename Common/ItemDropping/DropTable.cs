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

		if (choice == 0)
		{
			items = ItemDatabase.GetItemByType<Gear>();
		}
		else if (choice == 1)
		{
			items = ItemDatabase.GetItemByType<CurrencyShard>();
		}
		else
		{
			items = ItemDatabase.GetItemByType<Map>().Where(x => (x.Item.ModItem as Map).CanDrop);
		}

		if (forceRarity != (ItemRarity)(-1))
		{
			items = items.Where(x => x.Rarity == forceRarity);
		}

		return RollList(itemLevel, dropRarityModifier, items.ToList());
	}

	public static ItemDatabase.ItemRecord RollList(int itemLevel, float dropRarityModifier, List<ItemDatabase.ItemRecord> filteredGear)
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
