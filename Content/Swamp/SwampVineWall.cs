using PathOfTerraria.Content.Dusts;

namespace PathOfTerraria.Content.Swamp;

public class SwampVineWall : ModWall
{
	public override void SetStaticDefaults()
	{
		Main.wallHouse[Type] = false;

		DustType = ModContent.DustType<DarkMossDust>();
		AddMapEntry(new Color(0, 44, 0));
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = fail ? 1 : 3;
	}
}