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

		// Check if heldItem is valid and not air
		if (heldItem == null || heldItem.IsAir)
		{
			return false;
		}

		try
		{
			PoTInstanceItemData globalData = heldItem.GetInstanceData();
			if (globalData != null && globalData.Rarity == ItemRarity.Magic)
			{
				return base.CanRightClick();
			}
		}
		catch (KeyNotFoundException)
		{
			// Log the issue for debugging (optional)
			Mod.Logger.Warn($"PoTInstanceItemData not found for item: {heldItem.Name}");
		}

		return false;
	}

	public override void RightClick(Player player)
	{
		PoTItemHelper.Roll(player.HeldItem, player.HeldItem.GetInstanceData().RealLevel);
	}
}
