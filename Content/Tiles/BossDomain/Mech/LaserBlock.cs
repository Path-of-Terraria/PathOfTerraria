using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mech;

internal class LaserBlock : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileBrick[Type] = true;
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = false;
		Main.tileLighted[Type] = true;

		TileID.Sets.DrawsWalls[Type] = true;

		AddMapEntry(Color.Red);

		DustType = DustID.Firework_Red;
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		(r, g, b) = (0.5f, 0.04f, 0.06f);
	}
}