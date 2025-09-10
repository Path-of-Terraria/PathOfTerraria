using System.Collections.Generic;
using Terraria.GameContent.ItemDropRules;

namespace PathOfTerraria.Common.Items;

/// <summary>
/// Allows arbitrary additions to <see cref="GlobalItem.ModifyItemLoot(Item, ItemLoot)"/> drop pools. Meant to be used in <see cref="ModType.SetStaticDefaults"/>.<br/>
/// Copied from the same implementation in Spirit Reforged: https://github.com/GabeHasWon/SpiritReforged/blob/master/Common/ItemCommon/ItemLootDatabase.cs
/// </summary>
internal class ItemLootDatabase : GlobalItem
{
	public delegate void ModifyLoot(ref ItemLoot itemLoot);

	public readonly record struct ItemLootDrop(int ItemType, IItemDropRule Rule);

	internal static readonly Dictionary<int, ModifyLoot> LootDelegates = [];
	internal static List<ItemLootDrop> ItemRules = [];

	public static void AddItemRule(int itemType, IItemDropRule rule)
	{
		ItemRules.Add(new ItemLootDrop(itemType, rule));
	}

	public static void ModifyItemRule(int itemType, ModifyLoot rule)
	{
		LootDelegates.Add(itemType, rule);
	}

	public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
	{
		foreach (ItemLootDrop rule in ItemRules)
		{
			if (item.type == rule.ItemType)
			{
				itemLoot.Add(rule.Rule);
			}
		}

		if (LootDelegates.TryGetValue(item.type, out ModifyLoot del))
		{
			del.Invoke(ref itemLoot);
		}
	}
}
