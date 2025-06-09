using Terraria.ID;
using Terraria.Social.Steam;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mushroom;

internal class MushroomGrowths : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileCut[Type] = true;
		Main.tileLighted[Type] = true;
		Main.tileBrick[Type] = true;

		DustType = DustID.GlowingMushroom;
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		Tile tile = Main.tile[i, j];

		float sine = MathF.Sin(Main.GameUpdateCount * 0.01f + i * 0.5f + j * 0.5f);
		float str = Main.rand.Next(28, 42) * 0.005f;
		str += (270 - Main.mouseTextColor) / 1000f + sine;

		float mul = sine;

		if (tile.TileColor == PaintID.None)
		{
			r = 0f;
			g = 0.1f + str / 4f * mul;
			b = 0.5f * mul;
		}
		else
		{
			Color color2 = WorldGen.paintColor(tile.TileColor) * mul;
			r = color2.R / 400f;
			g = color2.G / 400f;
			b = color2.B / 400f;
		}
	}
}
