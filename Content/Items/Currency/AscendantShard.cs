using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using System.Diagnostics.CodeAnalysis;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Currency;

/// <summary>
/// A currency shard that can be used to add an affix to a magic or rare item.
/// </summary>
public class AscendantShard : CurrencyShard
{
	protected override void SetStaticData()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 500f;
		staticData.MinDropItemLevel = 25;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.rare = ItemRarityID.Blue;
	}

	public override bool CanUseInPouch(Item slotItem, [NotNullWhen(false)] out string failKey)
	{
		if (!DefaultValidityCheck(slotItem, out failKey))
		{
			return false;
		}

		ItemRarity rare = slotItem.GetInstanceData().Rarity;

		if (rare != ItemRarity.Magic && rare != ItemRarity.Rare)
		{
			failKey = "NotRareOrMagic";
			return false;
		}

		if (PoTItemHelper.HasMaxAffixesForRarity(slotItem))
		{
			failKey = "MaxAffixes";
			return false;
		}

		failKey = null;
		return true;
	}

	public override void RightClick(Player player)
	{
		base.RightClick(player);
		PoTItemHelper.SetMouseItemToHeldItem(player);
	}

	public override void ApplyToItem(Item slotItem)
	{
		PoTItemHelper.AddNewAffix(slotItem);
	}
}
