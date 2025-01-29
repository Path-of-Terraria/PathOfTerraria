using PathOfTerraria.Content.Tiles.Furniture;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Placeable;

public class GrovetenderTrophyItem : ModItem
{
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<GrovetenderTrophy>());

		Item.rare = ItemRarityID.Blue;
		Item.width = 32;
		Item.height = 30;
	}
}