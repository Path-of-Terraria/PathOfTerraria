using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Content.Items.Consumables.Maps;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.Utilities;
using GearItem = PathOfTerraria.Content.Items.Gear.Gear;

namespace PathOfTerraria.Content.Items.Currency;

/// <summary>
/// A currency shard that corrupts an item unpredictably.
/// Can add new affixes, reroll the item, or nothing can happen.
/// Once items are corrupted they can no longer be modified
/// </summary>
public class CorruptShard : CurrencyShard
{
	protected override void SetStaticData()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 500f;
		staticData.MinDropItemLevel = 5;
	}

	public override bool CanRightClick()
	{
		return base.CanRightClick() && Main.LocalPlayer.selectedItem == 58;
	}

	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Silk);
		Item.Size = new Vector2(30, 28);
		Item.rare = ItemRarityID.Green;
		Item.consumable = true; // Purely for the tooltip line
	}

	public override void RightClick(Player player)
	{
		PoTInstanceItemData data = player.HeldItem.GetInstanceData();
		data.Corrupted = true;

		if (Main.rand.NextBool(2)) // 50% chance to do nothing
		{
			PoTItemHelper.SetMouseItemToHeldItem(player);
			return;
		}

		if (data.Rarity == ItemRarity.Unique)
		{
			bool delevel = Main.rand.NextBool(2);

			if (delevel)
			{
				IEnumerable<ItemDatabase.ItemRecord> gear = ItemDatabase.AllItems.Where(x => x.Item.ModItem is GearItem &&
					x.Item.GetInstanceData().ItemType == data.ItemType && x.Rarity == ItemRarity.Rare);
				int count = gear.Count();

				if (count == 0)
				{
					return;
				}

				var chosenItem = gear.ElementAt(Main.rand.Next(count)).Item.ModItem as GearItem;
				int oldLevel = data.RealLevel;

				player.HeldItem.SetDefaults(chosenItem.Type);
				data = player.HeldItem.GetInstanceData();
				data.Rarity = ItemRarity.Rare;
				data.RealLevel = oldLevel;
				PoTItemHelper.Roll(player.HeldItem, data.RealLevel);
				data.Corrupted = true;
			}
			else
			{
				AddAffix(data, player.HeldItem);
			}
		}
		else
		{
			AddAffix(data, player.HeldItem);
		}

		PoTItemHelper.SetMouseItemToHeldItem(player);
	}

	private static void AddAffix(PoTInstanceItemData data, Item item)
	{
		WeightedRandom<ItemAffix> affixes = new();

		if (item.ModItem is not Map)
		{
			affixes.Add((ItemAffix)Affix.CreateAffix<BaseLifeAffix>(10, 20), 1);
			affixes.Add((ItemAffix)Affix.CreateAffix<DefenseItemAffix>(4, 6), 1);
			affixes.Add((ItemAffix)Affix.CreateAffix<IncreasedAttackSpeedAffix>(5), 0.01f);
		}
		else
		{
			affixes.Add(GenerateMapAffix<MapDamageAffix>(20, 35, 5, 7.5f), 1);
			affixes.Add(GenerateMapAffix<MapMobCritChanceAffix>(30, 50, 10, 14), 0.5f);
		}

		ItemAffix chosenAffix = affixes.Get();
		chosenAffix.IsCorruptedAffix = true;
		chosenAffix.Tier = 0;
		data.Affixes.Add(chosenAffix);
	}

	/// <summary>
	/// Generates a map affix with the given value and strength ranges. Value and strength will correspond to each other.
	/// </summary>
	private static MapAffix GenerateMapAffix<T>(float min, float max, float strengthMin, float strengthMax) where T : MapAffix
	{
		float factor = Main.rand.NextFloat();
		float strength = MathHelper.Lerp(strengthMin, strengthMax, factor);
		float value = MathHelper.Lerp(min, max, factor);
		var mapAffix = (MapAffix)Affix.CreateAffix<MapDamageAffix>(value);
		mapAffix.Strength = strength;
		return mapAffix;
	}
}
