using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Currency;

internal class AscendantShard : CurrencyShard
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.rare = ItemRarityID.Blue;
	}

	public override bool CanRightClick()
	{
		Item heldItem = Main.LocalPlayer.HeldItem;
		if (!heldItem.TryGetGlobalItem(out PoTGlobalItem _))
		{
			return false;
		}
		
		if (heldItem.GetInstanceData().Rarity != ItemRarity.Magic &&
		    heldItem.GetInstanceData().Rarity != ItemRarity.Rare)
		{
			Main.NewText("This item can only be used on Magic or Rare items.");
			return false;
		}

		if (PoTItemHelper.HasMaxAffixesForRarity(heldItem))
		{
			Main.NewText("This item has maximum affixes already");
			return false;
		}
		
		return base.CanRightClick();
	}

	public override void RightClick(Player player)
	{
		if (player.HeldItem == null)
		{
			return;
		}
		
		PoTItemHelper.AddNewAffix(player.HeldItem);
	}
}
