using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mech;

internal class BrokenSoftGlowplateWall : ModWall
{
	public override void SetStaticDefaults()
	{
		Main.wallHouse[Type] = false;
		Main.wallLight[Type] = true;

		DustType = DustID.BlueMoss;
		AddMapEntry(new Color(18, 40, 66));
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		(r, g, b) = (0.1f, 0.3f, 0.45f);
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = fail ? 1 : 3;
	}
}
