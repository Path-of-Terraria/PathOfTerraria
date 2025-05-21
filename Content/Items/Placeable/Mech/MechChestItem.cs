using PathOfTerraria.Content.Tiles.BossDomain.Mech;

namespace PathOfTerraria.Content.Items.Placeable.Mech;

public class MechChestItem : ModItem
{
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<MechChest>());
		Item.width = 32;
		Item.height = 26;
		Item.value = 0;
	}
}