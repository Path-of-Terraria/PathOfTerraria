﻿using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Currency;

/// <summary>
/// A currency shard that can be used to turn a magic or rare item into a normal item.
/// </summary>
internal class MysticShard : CurrencyShard
{
	protected override void SetStaticData()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 100f;
		staticData.MinDropItemLevel = 15;
	}

	public override bool CanRightClick()
	{
		PoTInstanceItemData data = Main.LocalPlayer.HeldItem.GetInstanceData();
		return base.CanRightClick() && data.Rarity is ItemRarity.Normal;
	}

	public override void RightClick(Player player)
	{
		PoTInstanceItemData data = player.HeldItem.GetInstanceData();
		data.Rarity = ItemRarity.Rare;
		data.Affixes = [];
		PoTItemHelper.Roll(player.HeldItem, data.RealLevel);
		PoTItemHelper.SetMouseItemToHeldItem(player);
	}
}
