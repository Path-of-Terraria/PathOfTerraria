using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using System.Diagnostics.CodeAnalysis;

namespace PathOfTerraria.Content.Items.Currency;

/// <summary>
/// A currency shard that will reroll the values of the affixes of an item.
/// </summary>
public class RadiantShard : CurrencyShard
{
	protected override void SetStaticData()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 250f;
		staticData.MinDropItemLevel = 25;
	}

	public override bool CanUseInPouch(Item slotItem, [NotNullWhen(false)] out string failKey)
	{
		if (!DefaultValidityCheck(slotItem, out failKey))
		{
			return false;
		}

		if (slotItem.GetInstanceData().Rarity is ItemRarity.Normal or ItemRarity.Unique)
		{
			failKey = "NotRareOrMagic";
			return false;
		}

		return true;
	}

	public override void ApplyToItem(Item slotItem)
	{
		PoTItemHelper.RerollAffixValues(slotItem);
	}
}
