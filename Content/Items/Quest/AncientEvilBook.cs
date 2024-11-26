using Terraria.ID;

namespace PathOfTerraria.Content.Items.Quest;

internal class AncientEvilBook : ModItem
{
	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Silk);
		Item.Size = new Vector2(30, 36);
		Item.rare = ItemRarityID.Quest;
		Item.questItem = true;
	}
}
