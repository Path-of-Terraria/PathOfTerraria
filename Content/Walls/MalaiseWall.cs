using Terraria.ID;

namespace PathOfTerraria.Content.Walls;

public class MalaiseWall : ModWall
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

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		float sine = MathF.Sin((i + j) * 20 + Main.GameUpdateCount * 0.06f) * 0.35f;
		(r, g, b) = (sine * 0.67f, sine * 0.2f, sine * 0.9f);
	}
}