using PathOfTerraria.Content.Tiles.BossDomain.Mech;

namespace PathOfTerraria.Content.Items.Placeable.Mech;

public class MechLampItem : ModItem
{
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<MechLamp>());
		Item.width = 12;
		Item.height = 32;
		Item.value = 0;
	}
}