using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using System.Diagnostics.CodeAnalysis;

namespace PathOfTerraria.Content.Items.Currency;

/// <summary>
/// A currency shard that can be used to reroll the affixes of a magic item.
/// </summary>
public class GlimmeringShard : CurrencyShard
{
	protected override void SetStaticData()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 5000f;
	}

	public override bool CanUseInPouch(Item slotItem, [NotNullWhen(false)] out string failKey)
	{
		if (!DefaultValidityCheck(slotItem, out failKey))
		{
			return false;
		}

		if (slotItem.GetInstanceData().Rarity != ItemRarity.Magic)
		{
			failKey = "NotMagic";
			return false;
		}

		return true;
	}

	public override void ApplyToItem(Item slotItem)
	{
		PoTItemHelper.Roll(slotItem, slotItem.GetInstanceData().RealLevel);
	}
}
