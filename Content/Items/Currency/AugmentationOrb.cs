using PathOfTerraria.Core.Items;
using System.Diagnostics.CodeAnalysis;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Currency;

//Use CurrencyShard so this can be added to the currency drop table
internal class AugmentationOrb : CurrencyShard
{
	public override bool SupportsMouseItemTargeting => false;
	public override bool SupportsPouchCrafting => false;

	public override void SetStaticDefaults()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 100f;
		staticData.MinDropItemLevel = 40;
	}

	public override void SetDefaults()
	{
		Item.maxStack = Item.CommonMaxStack;
		Item.rare = ItemRarityID.Orange;
		Item.value = Item.sellPrice(gold: 2, silver: 50);
	}

	public override bool CanRightClick()
	{
		return false;
	}

	// These hooks can't be used since this isn't a standard crafting currency.
	public override bool CanUseInPouch(Item slotItem, [NotNullWhen(false)] out string failKey)
	{
		failKey = "Invalid";
		return false;
	}

	public override void ApplyToItem(Item slotItem)
	{
	}
}
