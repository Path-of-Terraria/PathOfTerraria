using Terraria.ID;

namespace PathOfTerraria.Content.Walls;

public class PurpleCloudWall : ModWall
{
	public override void SetStaticDefaults()
	{
		Main.wallHouse[Type] = false;
		Main.wallLight[Type] = true;

		DustType = DustID.PurpleMoss;
		AddMapEntry(new Color(14, 6, 35));
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = fail ? 1 : 3;
	}
}