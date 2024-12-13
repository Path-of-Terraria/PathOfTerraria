using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using System.Linq;

namespace PathOfTerraria.Common.ItemDropping;

internal class DropTable
{
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
