using Terraria.ID;

namespace PathOfTerraria.Content.Items.BossDomain;

internal class PrismShardItem : ModItem
{
	public override void SetStaticDefaults()
	{
		Item.ResearchUnlockCount = 30;
	}

	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Silk);
		Item.Size = new Vector2(26, 20);
		Item.rare = ItemRarityID.Green;
		Item.value = Item.buyPrice(0, 0, 10);
	}
}
