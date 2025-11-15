using Terraria.ID;

namespace PathOfTerraria.Content.Items.Quest;

internal class RoyalJellyCore : ModItem
{
	public override void SetDefaults()
	{
		Item.Size = new Vector2(34, 38);
		Item.rare = ItemRarityID.Quest;
		Item.questItem = true;
	}
}