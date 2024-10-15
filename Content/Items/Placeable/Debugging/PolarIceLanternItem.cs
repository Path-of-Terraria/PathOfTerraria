#if DEBUG
using PathOfTerraria.Content.Tiles.BossDomain;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Placeable.Debugging;

internal class PolarIceLanternItem : ModItem
{
	public override void SetStaticDefaults()
	{
		ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true;
	}

	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<PolarIceLantern>());
		Item.width = 16;
		Item.height = 32;
		Item.rare = ItemRarityID.Blue;
	}
}
#endif