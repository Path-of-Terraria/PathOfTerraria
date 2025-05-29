using PathOfTerraria.Content.Tiles.BossDomain;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Placeable.Debugging;

internal class LihzahrdGuardItem : ModItem
{
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<LihzahrdGuard>());
		Item.width = 16;
		Item.height = 16;
		Item.rare = ItemRarityID.Blue;
	}
}
