using PathOfTerraria.Common.Tiles;
using PathOfTerraria.Common.World.Utilities;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.Maps.Swamp;

internal class SwampWeed : ModTile
{
	public static void Place(int i, int j, int frame)
	{
		if (TileInvalid(i, j) && TileInvalid(i, j + 1))
		{
			WorldGen.TileFrame(i, j + 1);
			return;
		}

		Tile weed = Main.tile[i, j];
		weed.HasTile = true;
        weed.TileType = (ushort)ModContent.TileType<SwampWeed>();
		weed.TileFrameX = (short)(18 * frame);
		weed.TileFrameNumber = Main.rand.NextBool(70) ? 1 : 0;
		WorldGen.TileFrame(i, j);

		static bool TileInvalid(int i, int j)
		{
			Tile tile = Main.tile[i, j];
			return tile.HasTile || tile.LiquidAmount < 100;
		}
	}

	public override void SetStaticDefaults()
	{
		Main.tileCut[Type] = true;
		Main.tileSolid[Type] = false;
		Main.tileLighted[Type] = true;

		DustType = DustID.Grass;

		AddMapEntry(new Color(30, 81, 62));
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		if (Main.tile[i, j].TileFrameNumber != 0)
		{
			(r, g, b) = (0.25f, 0.25f, 0.075f);
		}
	}

	public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
	{
		Tile tile = Main.tile[i, j];
		Tile above = Main.tile[i, j - 1];

		if (!above.HasTile || above.TileType != Type)
		{
			tile.TileFrameY = (short)(54 + 18 * Main.rand.Next(2));
		}
		else
		{
			tile.TileFrameY = (short)(18 * Main.rand.Next(3));
		}

		return false;
	}

	public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
	{
		if (Main.tile[i, j].TileFrameNumber != 0)
		{
			Vector2 position = TileExtensions.DrawPosition(i, j) + TileOffset(i, j);
			spriteBatch.Draw(TextureAssets.Tile[Type].Value, position.Floor(), new Rectangle(56, 18 * ((i + j) % 3), 16, 16), Color.White);
		}
	}

	public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
	{
		Tile tile = Main.tile[i, j];
		Vector2 position = TileExtensions.DrawPosition(i, j) + TileOffset(i, j);
		spriteBatch.Draw(TextureAssets.Tile[Type].Value, position.Floor(), new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16), Lighting.GetColor(i, j));

		return false;
	}

	public static Vector2 TileOffset(int i, int j)
	{
		int distance = j;

		for (int y = j; y < j + 3; ++y)
		{
			if (WorldUtilities.SolidTile(i, y))
			{
				distance = y;
			}
		}

		return new Vector2(MathF.Sin(i * 1.423f + j * 0.6f + Main.GameUpdateCount * 0.04f) * 4, 0) * (1 - (distance - j) / 3f);
	}
}
