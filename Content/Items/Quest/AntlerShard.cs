using Terraria.ID;

namespace PathOfTerraria.Content.Items.Quest;

internal class AntlerShard : ModItem
{
	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Silk);
		Item.Size = new Vector2(16, 16);
		Item.rare = ItemRarityID.Quest;
		Item.questItem = true;
	}
}
