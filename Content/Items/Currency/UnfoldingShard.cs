using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Currency;

internal class UnfoldingShard : CurrencyShard
{
	public override bool CanRightClick()
	{
		return base.CanRightClick() && Main.LocalPlayer.HeldItem.GetInstanceData().Rarity == ItemRarity.Normal;
	}

	public override void RightClick(Player player)
	{
		PoTInstanceItemData data = player.HeldItem.GetInstanceData();
		data.Rarity = ItemRarity.Magic;
		PoTItemHelper.Roll(player.HeldItem, data.RealLevel);

		if (player.selectedItem == 58)
		{
			Main.mouseItem = player.HeldItem;
		}
	}
}
