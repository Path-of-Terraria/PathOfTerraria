using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using System.Diagnostics.CodeAnalysis;

namespace PathOfTerraria.Content.Items.Currency;

/// <summary>
/// A currency shard that removes a random affix.
/// </summary>
public class SeveranceShard : CurrencyShard
{
	protected override int FrameCount => 6;

	protected override void SetStaticData()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 750f;
	}

	public override bool CanUseInPouch(Item slotItem, [NotNullWhen(false)] out string failKey)
	{
		if (!DefaultValidityCheck(slotItem, out failKey))
		{
			return false;
		}

		PoTInstanceItemData data = slotItem.GetInstanceData();

		if (data.Rarity == ItemRarity.Normal)
		{
			failKey = "IsNormal";
			return false;
		}

		if (data.Affixes.Count == 0)
		{
			failKey = "NoAffixes";
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
		data.Affixes.RemoveAt(Main.rand.Next(data.Affixes.Count));
	}
}
