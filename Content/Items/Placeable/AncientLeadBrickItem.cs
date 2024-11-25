using PathOfTerraria.Content.Tiles.BossDomain;

namespace PathOfTerraria.Content.Items.Placeable;

public class AncientLeadBrickItem : ModItem
{
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<AncientLeadBrick>());
		Item.width = 60;
		Item.height = 38;
		Item.value = 0;
	}
}