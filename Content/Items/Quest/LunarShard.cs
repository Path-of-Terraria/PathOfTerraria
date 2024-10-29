using Terraria.ID;

namespace PathOfTerraria.Content.Items.Quest;

internal class LunarShard : ModItem
{
	public override void SetStaticDefaults()
	{
		Item.ResearchUnlockCount = 1;
	}

	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Silk);
		Item.rare = ItemRarityID.Quest;
		Item.Size = new Vector2(24, 18);
	}
}
