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
		Main.tileBlockLight[Type] = true;
		Main.tileCracked[Type] = true;

		Main.tileMerge[Type][TileID.Dirt] = true;
		Main.tileMerge[TileID.Dirt][Type] = true;		
		Main.tileMerge[Type][TileID.Pearlstone] = true;
		Main.tileMerge[TileID.Pearlstone][Type] = true;

		AddMapEntry(new Color(174, 110, 229));

		DustType = DustID.PurpleCrystalShard;
		HitSound = SoundID.Shatter;
	}
}

internal class BrittleCrystalItem : ModItem
{
	public override void SetDefaults()
	{
		Item.DefaultToPlaceableTile(ModContent.TileType<BrittleCrystal>());
	}
}