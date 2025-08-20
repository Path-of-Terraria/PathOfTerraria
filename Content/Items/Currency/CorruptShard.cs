using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
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

		Item.stack--;

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
				AddAffix(data);
			}
		}
		else
		{
			AddAffix(data);
		}

		PoTItemHelper.SetMouseItemToHeldItem(player);
	}

	private static void AddAffix(PoTInstanceItemData data)
	{
		WeightedRandom<ItemAffix> affixes = new();
		affixes.Add((ItemAffix)Affix.CreateAffix<FlatLifeAffix>(10, 20), 1);
		affixes.Add((ItemAffix)Affix.CreateAffix<DefenseItemAffix>(4, 6), 1);
		affixes.Add((ItemAffix)Affix.CreateAffix<IncreasedAttackSpeedAffix>(5), 0.01f);

		ItemAffix chosenAffix = affixes.Get();
		chosenAffix.IsCorruptedAffix = true;
		data.Affixes.Add(chosenAffix);
	}
}
