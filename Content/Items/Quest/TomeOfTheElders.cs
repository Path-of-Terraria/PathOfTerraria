using Terraria.ID;

namespace PathOfTerraria.Content.Items.Quest;

internal class TomeOfTheElders : ModItem
{
	public override void SetStaticDefaults()
	{
		Item.CloneDefaults(ItemID.Silk);
		Item.Size = new Vector2(46, 28);
		Item.rare = ItemRarityID.Purple;
	}
}
