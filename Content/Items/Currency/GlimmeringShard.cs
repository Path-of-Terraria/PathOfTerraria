using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Currency;

internal class GlimmeringShard : CurrencyShard
{
	public override bool CanRightClick()
	{
		return base.CanRightClick() && Main.LocalPlayer.HeldItem.GetInstanceData().Rarity == ItemRarity.Magic;
	}

	public override void RightClick(Player player)
	{
		PoTItemHelper.Roll(player.HeldItem, player.HeldItem.GetInstanceData().RealLevel);
	}
}
