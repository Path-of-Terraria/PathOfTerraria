using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;

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

	public override bool CanRightClick()
	{
		return base.CanRightClick() && Main.LocalPlayer.HeldItem.GetInstanceData().Rarity is ItemRarity.Normal;
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
