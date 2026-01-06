using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Tiles;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.Maps.Swamp;

internal class SwampMoss : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileBrick[Type] = true;
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = false;
		Main.tileBlendAll[Type] = true;

		TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;
		TileID.Sets.DrawsWalls[Type] = true;

		AddMapEntry(new Color(56, 66, 66));

		DustType = DustID.Lead;
		HitSound = SoundID.Tink;
	}

	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Tile tile = Main.tile[i, j];
		Rectangle src = new(tile.TileFrameX, tile.TileFrameY, 16, 16);
		Draw(0);

		if (StaticNoise.Perlin.GetNoise(i * 22, j * 22) > 0.82f)
		{
			Draw(0, new Rectangle(i % 4 * 18, 90, 16, 16));
		}

		return false;

		//Draw(0);
		//Draw(1);
		//Draw(2);
		//Draw(3);

		//return false;

		void Draw(int offset, Rectangle? srcOverride = null)
		{
			Vector2 off = tile.LiquidAmount > 0 && srcOverride is not null ? new(0, MathF.Sin(Main.GameUpdateCount * 0.03f + i * MathHelper.PiOver4) * 2) : Vector2.Zero;
			Vector2 position = TileExtensions.DrawPosition(i, j) + off;
			Color color = Lighting.GetColor(i, j);
			
			if (tile.LiquidAmount <= 0 && srcOverride is null)
			{
				position.Y += 4;
				spriteBatch.Draw(TextureAssets.Tile[Type].Value, position.Floor(), srcOverride ?? src with { Y = src.Y + 90 * offset }, color);
			}

			spriteBatch.Draw(TextureAssets.Tile[Type].Value, position.Floor(), srcOverride ?? src with { Y = src.Y + 90 * offset }, color);
		}
	}
}