using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Currency;

internal class GlimmeringShard : CurrencyShard
{
	public override bool CanRightClick()
	{
		if (Main.LocalPlayer.HeldItem.GetInstanceData().Rarity == ItemRarity.Magic)
		{
			return base.CanRightClick();
		}

		Main.NewText("Item must be of Magic rarity");
		return false;
	}

	public override void RightClick(Player player)
	{
		PoTItemHelper.Roll(player.HeldItem, player.HeldItem.GetInstanceData().RealLevel);
	}
}
