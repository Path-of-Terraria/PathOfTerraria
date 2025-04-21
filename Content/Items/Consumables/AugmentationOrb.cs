using Terraria.ID;

namespace PathOfTerraria.Content.Items.Consumables;

internal class AugmentationOrb : ModItem
{
	public override void SetDefaults()
	{
		Item.maxStack = Item.CommonMaxStack;
		Item.rare = ItemRarityID.Orange;
		Item.value = Item.sellPrice(gold: 2, silver: 50);
	}
}