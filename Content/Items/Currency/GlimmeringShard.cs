using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Currency;

/// <summary>
/// A currency shard that can be used to reroll the affixes of a magic item.
/// </summary>
public class GlimmeringShard : CurrencyShard
{
	protected override void SetStaticData()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 5000f;
	}

	public override bool CanRightClick()
	{
		Item heldItem = Main.LocalPlayer.HeldItem;

		if (heldItem.GetInstanceData().Rarity == ItemRarity.Magic && !heldItem.IsAir)
		{
			return base.CanRightClick();
		}

		return false;
	}

	public override void RightClick(Player player)
	{
		PoTItemHelper.Roll(player.HeldItem, player.HeldItem.GetInstanceData().RealLevel);
	}
}
