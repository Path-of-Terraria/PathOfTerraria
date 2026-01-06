using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.Maps.Swamp;

internal class SwampGrass : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileBrick[Type] = true;
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = false;
		Main.tileBlendAll[Type] = true;

		TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;
		TileID.Sets.Grass[Type] = true;
		TileID.Sets.NeedsGrassFraming[Type] = true;
		TileID.Sets.NeedsGrassFramingDirt[Type] = TileID.Mud;

		AddMapEntry(new Color(37, 91, 15));

		DustType = DustID.Grass;
		HitSound = SoundID.Tink;
	}
}