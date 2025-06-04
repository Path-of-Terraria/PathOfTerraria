using Terraria.ID;

namespace PathOfTerraria.Content.Walls;

public class MushroomGrowthWall : ModWall
{
	public override void SetStaticDefaults()
	{
		Main.wallHouse[Type] = false;
		Main.wallLight[Type] = true;

		DustType = DustID.GlowingMushroom;
		AddMapEntry(new Color(14, 6, 35));
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = fail ? 1 : 3;
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		Tile tile = Main.tile[i, j];

		if (tile.HasTile)
		{
			return;
		}

		float str = Main.rand.Next(28, 42) * 0.005f;
		str += (270 - Main.mouseTextColor) / 1000f + MathF.Sin(Main.GameUpdateCount * 0.02f + i + j) * 1f;

		if (tile.TileColor == PaintID.None)
		{
			r = 0f;
			g = 0.1f + str / 2f;
			b = 0.5f;
		}
		else
		{
			Color color2 = WorldGen.paintColor(tile.TileColor);
			r = color2.R / 400f;
			g = color2.G / 400f;
			b = color2.B / 400f;
		}
	}
}