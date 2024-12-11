using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Currency;

/// <summary>
/// A currency shard that will turn a normal item into magic
/// </summary>
internal class UnfoldingShard : CurrencyShard
{
	protected override void SetStaticData()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 5000f;
	}

	public override bool CanRightClick()
	{
		return base.CanRightClick() && Main.LocalPlayer.HeldItem.GetInstanceData().Rarity == ItemRarity.Normal;
	}

	public override void RightClick(Player player)
	{
		PoTInstanceItemData data = player.HeldItem.GetInstanceData();
		data.Rarity = ItemRarity.Magic;
		PoTItemHelper.Roll(player.HeldItem, data.RealLevel);
		PoTItemHelper.SetMouseItemToHeldItem(player);
	}
}
