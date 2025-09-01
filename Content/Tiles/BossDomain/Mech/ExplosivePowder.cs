using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mech;

internal class ExplosivePowder : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileBrick[Type] = true;
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = false;

		AddMapEntry(new Color(142, 121, 121));

		DustType = DustID.Ebonwood;
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = 8;
	}
}