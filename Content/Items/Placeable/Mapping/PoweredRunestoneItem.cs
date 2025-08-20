using PathOfTerraria.Content.Tiles.Maps.Forest;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Placeable.Mapping;

public class PoweredRunestoneItem : ModItem
{
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<PoweredRunestone>());
		Item.rare = ItemRarityID.Blue;
		Item.width = 16;
		Item.height = 16;
		Item.value = Item.buyPrice(0, 0, 0, 15);
	}
}