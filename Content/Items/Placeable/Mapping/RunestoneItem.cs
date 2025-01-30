using PathOfTerraria.Content.Tiles.Maps.Forest;

namespace PathOfTerraria.Content.Items.Placeable.Mapping;

public class RunestoneItem : ModItem
{
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<Runestone>());
		Item.width = 16;
		Item.height = 16;
		Item.value = 0;
	}
}