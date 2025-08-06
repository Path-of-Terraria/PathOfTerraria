using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;

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

		if (heldItem == null || heldItem.IsAir)
		{
			return false;
		}

		heldItem.TryGetGlobalItem(out PoTInstanceItemData data);
		if (data != null && data.Rarity == ItemRarity.Magic)
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
