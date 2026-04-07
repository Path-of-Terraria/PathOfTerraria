using Terraria.ID;

namespace PathOfTerraria.Content.Swamp;

public class MangroveWall : ModWall
{
	public override void SetStaticDefaults()
	{
		Main.wallHouse[Type] = false;

		DustType = DustID.Pearlwood;
		AddMapEntry(new Color(118, 93, 68));
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = fail ? 1 : 3;
	}
}