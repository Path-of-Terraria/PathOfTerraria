using PathOfTerraria.Common.Tiles;
using Terraria.GameContent;

namespace PathOfTerraria.Content.Tiles.BossDomain.Moon;

internal class MiniVortex : GasTile
{
	public override Color TileColor => new(0, 242, 230);
	public override int SineDirection => -1;

	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Tile tile = Main.tile[i, j];
		Texture2D tex = TextureAssets.Tile[tile.TileType].Value;
		Rectangle src = tile.BasicFrame();
		SpriteEffects effects = i % 2 == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
		float sinY = MathF.Sin(i + j * 3f + Main.GameUpdateCount * 0.045f + MathHelper.PiOver2 * 0.39f);
		Vector2 sin = new Vector2(MathF.Sin(i * SineDirection * 0.8f + j + Main.GameUpdateCount * 0.03f), sinY) * 2;
		float scale = MathF.Sin(i * MathHelper.Pi + -j + Main.GameUpdateCount * 0.02f) * 0.4f + 0.6f;

		if (j % 2 == 0)
		{
			effects |= SpriteEffects.FlipVertically;
		}

		Vector2 position = TileExtensions.DrawPosition(i, j) + sin + new Vector2(8);
		spriteBatch.Draw(tex, position, src, Lighting.GetColor(i, j), 0f, src.Size() / 2f, scale, effects, 0);
		return false;
	}
}