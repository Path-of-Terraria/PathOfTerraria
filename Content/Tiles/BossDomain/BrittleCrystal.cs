using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class BrittleCrystal : ModTile
{
	public override void SetStaticDefaults()
	{
		TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
		TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;
		TileID.Sets.DrawsWalls[Type] = true;

		Main.tileBrick[Type] = true;
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = false;
		Main.tileCracked[Type] = true;

		AddMapEntry(new Color(174, 110, 229));

		DustType = DustID.PurpleCrystalShard;
	}
}

internal class BrittleCrystalItem : ModItem
{
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<BrittleCrystal>());
	}
}