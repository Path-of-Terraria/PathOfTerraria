using PathOfTerraria.Content.Tiles.Town;

namespace PathOfTerraria.Content.Items.Placeable;

internal class RavenStatueItem : ModItem
{
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<RavenStatue>());
		Item.width = 32;
		Item.height = 34;
	}
}
