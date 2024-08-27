using PathOfTerraria.Content.Tiles.Furniture;
using PathOfTerraria.Content.Tiles.Town;

namespace PathOfTerraria.Content.Items.Placeable;

public class MapDevice : ModItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.DefaultToPlaceableTile(ModContent.TileType<WallClock>());
		Item.width = 60;
		Item.height = 38;
		Item.value = 0;
	}
}