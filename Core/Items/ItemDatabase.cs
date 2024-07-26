using PathOfTerraria.Core.Systems.TreeSystem;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Core.Items;

/// <summary>
///		Keeps track of registered items.
/// </summary>
public sealed class ItemDatabase : ModSystem
{
	/// <summary>
	///		An item record within the database.
	/// </summary>
	public readonly record struct ItemRecord(float DropChance, Rarity Rarity, int ItemId);

	private static List<ItemRecord> items = [];

	private const float _magicFindPowerDecrease = 100f;

	public static IReadOnlyCollection<ItemRecord> AllItems => items;

	public override void PostSetupContent()
	{
		base.PostSetupContent();

		for (int i = 0; i < ItemID.Count; i++)
		{
			Item item = ContentSamples.ItemsByType[i];
			PoTStaticItemData staticData = item.GetStaticData();

			// If the drop chance is null (the default value), then this item does not
			// specify a drop chance AT ALL (DIFFERENT from 0%), and should not be
			// registered.  This is most prominent in things like vanilla items we
			// haven't intended to every drop.
			if (!staticData.DropChance.HasValue)
			{
				continue;
			}

			float dropChance = staticData.DropChance.Value;

			if (staticData.IsUnique)
			{
				AddItem(dropChance, Rarity.Unique, i);
			}
			else
			{
				// TODO: Hardcoded :(
				if (item.ModItem is not Jewel)
				{
					AddItem(dropChance * 0.7f, Rarity.Normal, i);
				}

				AddItem(dropChance * 0.25f, Rarity.Magic, i);
				AddItem(dropChance * 0.05f, Rarity.Rare, i);
			}
		}
	}

	public override void Unload()
	{
		base.Unload();

		items = null;
	}

	public static void AddItem(float dropChance, Rarity rarity, int itemId)
	{
		items.Add(new ItemRecord(dropChance, rarity, itemId));
	}

	public static void AddItemWithVariableRarity(float dropChance, int itemId)
	{
		AddItem(dropChance * 0.7f, Rarity.Normal, itemId);
		AddItem(dropChance * 0.25f, Rarity.Magic, itemId);
		AddItem(dropChance * 0.05f, Rarity.Rare, itemId);
	}

	public static float ApplyRarityModifier(float chance, float dropRarityModifier)
	{
		// this is just some arbitrary function from chat gpt, modified a little...
		// it is pretty hard to get all this down when we dont know all the items we will have n such;

		chance *= 100f; // to make it effective on <0.1; it works... ok?
		float powerDecrease = chance * (1 + dropRarityModifier / _magicFindPowerDecrease) /
							  (1 + chance * dropRarityModifier / _magicFindPowerDecrease);
		return powerDecrease;
	}
}
