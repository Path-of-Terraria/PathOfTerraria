using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class PusBlock : ModTile
{
	public override void SetStaticDefaults()
	{
		TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
		TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;
		TileID.Sets.CanBeDugByShovel[Type] = true;
		TileID.Sets.IceSkateSlippery[Type] = true;

		Main.tileBrick[Type] = true;
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = true;

		AddMapEntry(new Color(204, 168, 106));

		DustType = DustID.YellowStarfish;
		HitSound = SoundID.NPCHit1;
	}
}