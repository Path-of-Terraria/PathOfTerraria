using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Currency;

internal class RadiantShard : CurrencyShard
{
	public override bool CanRightClick()
	{
		return base.CanRightClick() && Main.LocalPlayer.HeldItem.GetInstanceData().Rarity != ItemRarity.Normal;
	}

	public override void RightClick(Player player)
	{
		PoTItemHelper.RerollAffixValues(player.HeldItem);
	}
}
