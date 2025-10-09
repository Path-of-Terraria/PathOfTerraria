using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using System.Diagnostics.CodeAnalysis;

namespace PathOfTerraria.Content.Items.Currency;

/// <summary>
/// A currency shard that can be used to turn a magic or rare item into a normal item.
/// </summary>
public class LimpidShard : CurrencyShard
{
	protected override void SetStaticData()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1000f;
		staticData.MinDropItemLevel = 10;
	}

	public override bool CanUseInPouch(Item slotItem, [NotNullWhen(false)] out string failKey)
	{
		if (!DefaultValidityCheck(slotItem, out failKey))
		{
			return false;
		}

		if (Main.LocalPlayer.HeldItem.GetInstanceData().Rarity is ItemRarity.Normal or ItemRarity.Unique)
		{
			failKey = "NotRareOrMagic";
			return false;
		}

		return true;
	}

	public override void RightClick(Player player)
	{
		ApplyChanges(player.HeldItem);
		PoTItemHelper.SetMouseItemToHeldItem(player);
	}

	public override void ApplyToItem(Item slotItem)
	{
		ApplyToItem(slotItem);
	}

	private static void ApplyChanges(Item item)
	{
		PoTInstanceItemData data = item.GetInstanceData();
		data.Rarity = ItemRarity.Normal;
		data.Affixes = [];
		PoTItemHelper.Roll(item, data.RealLevel);
	}
}
