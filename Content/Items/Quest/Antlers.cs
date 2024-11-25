using Terraria.ID;

namespace PathOfTerraria.Content.Items.Quest;

internal class Antlers : ModItem
{
	public override void SetDefaults()
	{
		Item.CloneDefaults(ItemID.Silk);
		Item.Size = new Vector2(48, 44);
		Item.rare = ItemRarityID.Quest;
		Item.questItem = true;
	}
}
