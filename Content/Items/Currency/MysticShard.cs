using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using System.Diagnostics.CodeAnalysis;

namespace PathOfTerraria.Content.Items.Currency;

/// <summary>
/// A currency shard that can be used to upgrade a normal item to rare.
/// </summary>
public class MysticShard : CurrencyShard
{
	protected override int FrameCount => 5;

	protected override void SetStaticData()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 500f;
		staticData.MinDropItemLevel = 15;
	}

	public override bool CanUseInPouch(Item slotItem, [NotNullWhen(false)] out string failKey)
	{
		if (!DefaultValidityCheck(slotItem, out failKey))
		{
			return false;
		}

		if (slotItem.GetInstanceData().Rarity != ItemRarity.Normal)
		{
			failKey = "NotNormal";
			return false;
		}

		return true;
	}

	public override void RightClick(Player player)
	{
		base.RightClick(player);
		PoTItemHelper.SetMouseItemToHeldItem(player);
	}

	public override void ApplyToItem(Item slotItem)
	{
		PoTInstanceItemData data = slotItem.GetInstanceData();
		data.Rarity = ItemRarity.Rare;
		data.Affixes = [];
		PoTItemHelper.Roll(slotItem, data.RealLevel);
	}
}
