using Terraria.ID;

namespace PathOfTerraria.Content.Swamp;

public class PerforatedDeepMossWall : ModWall
{
	public override void SetStaticDefaults()
	{
		Main.wallHouse[Type] = false;
		Main.wallLight[Type] = true;
		Main.wallBlend[Type] = ModContent.WallType<DeepMossWall>();

		WallID.Sets.AllowsWind[Type] = true;

		DustType = DustID.GreenMoss;
		AddMapEntry(new Color(10, 33, 13));
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = fail ? 1 : 3;
	}
}