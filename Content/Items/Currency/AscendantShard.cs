using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Currency;

/// <summary>
/// A currency shard that can be used to add an affix to a magic or rare item.
/// </summary>
public class AscendantShard : CurrencyShard
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

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

		ItemRarity rare = heldItem.GetInstanceData().Rarity;

		if (rare != ItemRarity.Magic && rare != ItemRarity.Rare)
		{
			//Main.NewText(Language.GetTextValue($"Mods.{PoTMod.ModName}.Misc.ShardNotifs.Ascendant.NotRareOrMagic"));
			return false;
		}

		if (PoTItemHelper.HasMaxAffixesForRarity(heldItem))
		{
			//Main.NewText(Language.GetTextValue($"Mods.{PoTMod.ModName}.Misc.ShardNotifs.Ascendant.MaxAffixes"));
			return false;
		}
		
		return base.CanRightClick();
	}

	public override void RightClick(Player player)
	{
		PoTItemHelper.AddNewAffix(player.HeldItem);
		PoTItemHelper.SetMouseItemToHeldItem(player);
	}
}
