using PathOfTerraria.Common.Tiles;
using Terraria.GameContent;

namespace PathOfTerraria.Content.Tiles.BossDomain.Moon;

internal class SolarFlare : GasTile
{
	public static Vector2[] Offsets = [Vector2.Zero, new Vector2(4, -4), new Vector2(-6, 8), new Vector2(6, 2), new Vector2(-8, -2)];
	public static Color[] OffsetColors = [Color.White, new Color(140, 140, 140) * 0.9f, new Color(255, 255, 40) * 0.7f, new Color(60, 60, 60) * 0.4f];

	public override Color TileColor => new(249, 75, 9);

	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Tile tile = Main.tile[i, j];
		Texture2D tex = TextureAssets.Tile[tile.TileType].Value;
		Rectangle src = tile.BasicFrame();
		src.X = 0;
		src = ModifySource(i, j, tile, src, 0);
		SpriteEffects effects = i % 2 == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

		float windPush = Main.instance.TilesRenderer.GetWindGridPush(i, j, 60, 0.1f);
		float sinY = MathF.Sin(i + j * 3f + Main.GameUpdateCount * 0.045f + MathHelper.PiOver2 * 0.39f);
		int repeats = tile.TileFrameX / 18;

		for (int k = 0; k < repeats + 1; ++k)
		{
			Vector2 position = TileExtensions.DrawPosition(i, j) + new Vector2(8 + windPush, 8) + Offsets[k] + GetSineWave(i + k * 17, j + k * 7, sinY);
			src = ModifySource(i, j + i, tile, src, k * 12);
			spriteBatch.Draw(tex, position, src, Color.Lerp(Lighting.GetColor(i, j), OffsetColors[k], 0.6f) * 0.7f, 0f, src.Size() / 2f, 1f, effects, 0);
		}

		return false;
	}

	private static Vector2 GetSineWave(int i, int j, float sinY)
	{
		return new Vector2(MathF.Sin(i * 0.8f + j + Main.GameUpdateCount * 0.03f), sinY) * 4;
	}

	private static Rectangle ModifySource(int i, int j, Tile tile, Rectangle src, int offset)
	{
		src.Y = (int)(tile.TileFrameY + 18 * (Main.GameUpdateCount + offset) / 80f + (i * 2 + j * 6)) * 18 % 54;
		return src;
	}
}