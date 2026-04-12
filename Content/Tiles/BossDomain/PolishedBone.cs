using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class PolishedBone : ModTile
{
	public override void SetStaticDefaults()
	{
		TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
		TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;

		Main.tileBrick[Type] = true;
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = true;

		AddMapEntry(new Color(204, 190, 159));

		DustType = DustID.Bone;
	}
}