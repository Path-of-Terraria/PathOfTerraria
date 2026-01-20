using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Tiles;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.Maps.Swamp;

internal class SwampMoss : ModTile, IAutoloadTileItem
{
	public override void SetStaticDefaults()
	{
		Main.tileBrick[Type] = true;
		Main.tileSolid[Type] = true;
		Main.tileBlockLight[Type] = true;
		Main.tileBlendAll[Type] = true;

		TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;
		TileID.Sets.DrawsWalls[Type] = true;

		AddMapEntry(new Color(56, 66, 66));

		DustType = DustID.Lead;
		HitSound = SoundID.Tink;
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		if (StaticNoise.Perlin.GetNoise(i * 22, j * 22) > 0.82f)
		{
			Tile tile = Main.tile[i, j];
			Rectangle srcOverride = new(i % 4 * 18, 90, 16, 16);
			Vector2 off = tile.LiquidAmount > 0 ? new(0, MathF.Sin(Main.GameUpdateCount * 0.03f + i * MathHelper.PiOver4) * 2) : Vector2.Zero;
			Vector2 position = TileExtensions.DrawPosition(i, j) + off;
			Color color = Lighting.GetColor(i, j);

			if (tile.IsActuated)
			{
				color = Color.Lerp(color, Color.Black, 0.6f);
			}
			
			if (tile.LiquidAmount <= 0)
			{
				position.Y += 4;
				spriteBatch.Draw(TextureAssets.Tile[Type].Value, position.Floor(), srcOverride, color);
			}

			spriteBatch.Draw(TextureAssets.Tile[Type].Value, position.Floor(), srcOverride, color);
		}
	}
}

internal class DeepMoss : SwampMoss, IAutoloadTileItem { }