using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Currency;

/// <summary>
/// A currency shard that can be used to turn a magic or rare item into a normal item.
/// </summary>
internal class LimpidShard : CurrencyShard
{
	public override bool CanRightClick()
	{
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
