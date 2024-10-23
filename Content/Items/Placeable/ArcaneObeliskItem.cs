using PathOfTerraria.Content.Tiles;

namespace PathOfTerraria.Content.Items.Placeable;

public class ArcaneObeliskItem : ModItem
{
	public override string Texture => base.Texture.Replace("Content", "Assets");

	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<ArcaneObeliskTile>());

		Item.width = 24;
		Item.height = 30;
	}
}