using PathOfTerraria.Content.Tiles.Furniture;

namespace PathOfTerraria.Content.Items.Placeable;

public class MapDevice : ModItem
{
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<MapDevicePlaceable>());
		Item.width = 60;
		Item.height = 38;
		Item.value = 0;
	}
}