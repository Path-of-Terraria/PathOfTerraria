using PathOfTerraria.Content.Tiles.Furniture;

namespace PathOfTerraria.Content.Items.Placeable;

public class MapDevice : ModItem
{
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<MapDevicePlaceable>());
		Item.width = 48;
		Item.height = 18;
		Item.value = 0;
	}
}