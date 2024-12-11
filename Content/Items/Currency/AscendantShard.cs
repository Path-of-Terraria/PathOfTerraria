using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Currency;

/// <summary>
/// A currency shard that can be used to add an affix to a magic or rare item.
/// </summary>
internal class AscendantShard : CurrencyShard
{
	public override void SetStaticDefaults()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 50;
		staticData.MinDropItemLevel = 25;
	}

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
		PoTItemHelper.AddNewAffix(player.HeldItem);
	}
}
