using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;

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

	public override bool CanRightClick()
	{
		Item heldItem = Main.LocalPlayer.HeldItem;
		if (heldItem == null || heldItem.IsAir)
		{
			return base.CanRightClick();
		}

		PoTInstanceItemData data = Main.LocalPlayer.HeldItem.GetInstanceData();
		return base.CanRightClick() && data.Rarity is ItemRarity.Magic or ItemRarity.Rare;
	}

	public override void RightClick(Player player)
	{
		PoTInstanceItemData data = player.HeldItem.GetInstanceData();
		data.Rarity = ItemRarity.Normal;
		data.Affixes = [];
		PoTItemHelper.Roll(player.HeldItem, data.RealLevel);
		PoTItemHelper.SetMouseItemToHeldItem(player);
	}
}
