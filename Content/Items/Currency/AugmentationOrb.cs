using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Currency;

//Use CurrencyShard so this can be added to the currency drop table
internal class AugmentationOrb : CurrencyShard
{
	public override void SetStaticDefaults()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 500f;
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
}