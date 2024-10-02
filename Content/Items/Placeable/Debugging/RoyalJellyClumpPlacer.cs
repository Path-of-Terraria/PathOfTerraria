using PathOfTerraria.Content.Tiles.BossDomain;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Placeable.Debugging;

internal class RoyalJellyClumpPlacer : ModItem
{
	public override void SetStaticDefaults()
	{
		ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;
	}

	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<RoyalHoneyClumpTile>());
	}
}
