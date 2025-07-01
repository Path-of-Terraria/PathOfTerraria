using PathOfTerraria.Common.Tiles;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.BossDomain.Moon;

internal abstract class GasTile : ModTile
{
	public abstract Color TileColor { get; }
	public abstract int Dust { get; }
	public virtual int SineDirection { get; }
	public virtual Color GlowColor => TileColor;

	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = false;
		Main.tileBlockLight[Type] = false;
		Main.tileLighted[Type] = true;

		AddMapEntry(TileColor);

		DustType = Dust;
		HitSound = SoundID.Item39;
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = 2;
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		(r, g, b) = (GlowColor.R / 255f, GlowColor.G / 255f, GlowColor.B / 255f);
	}

	public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
	{
		Tile self = Main.tile[i, j];
		self.TileFrameY = (short)(Main.rand.Next(3) * 18);
		return false;
	}

	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Tile tile = Main.tile[i, j];
		Texture2D tex = TextureAssets.Tile[tile.TileType].Value;
		Rectangle src = tile.BasicFrame();
		SpriteEffects effects = i % 2 == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
		float sinY = MathF.Sin(i + j * 3f + Main.GameUpdateCount * 0.045f + MathHelper.PiOver2 * 0.39f);
		float windPush = Main.instance.TilesRenderer.GetWindGridPush(i, j, 60, 0.1f);
		Vector2 sin = new Vector2(MathF.Sin(i * SineDirection * 0.8f + j + Main.GameUpdateCount * 0.03f) + windPush, sinY) * 2;

		if (j % 2 == 0)
		{
			effects |= SpriteEffects.FlipVertically;
		}

		Vector2 position = TileExtensions.DrawPosition(i, j) + sin + new Vector2(8);
		spriteBatch.Draw(tex, position, src, Lighting.GetColor(i, j), 0f, src.Size() / 2f, 1f, effects, 0);
		return false;
	}
}
