using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class AncientLeadBrick : ModTile
{
	public override void SetStaticDefaults()
	{
		TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
		TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;

		Main.tileBrick[Type] = true;
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = true;

		AddMapEntry(new Color(65, 54, 99));

		DustType = DustID.Lead;
	}

	public override bool CanKillTile(int i, int j, ref bool blockDamaged)
	{
		return NPC.downedDeerclops;
	}

	public override bool CanExplode(int i, int j)
	{
		return NPC.downedDeerclops;
	}

	public override bool CanReplace(int i, int j, int tileTypeBeingPlaced)
	{
		return NPC.downedDeerclops;
	}
}