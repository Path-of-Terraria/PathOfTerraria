using Terraria.ID;

namespace PathOfTerraria.Common.Tiles;

internal static class SlopeDrawing
{
	public static void DrawSloped(this ModTile _, int i, int j, Texture2D texture, Color color, Vector2 positionOffset, bool overrideFrame = false)
	{
		DrawSloped(i, j, texture, color, positionOffset, overrideFrame);
	}

	public static void DrawSloped(int i, int j, Texture2D texture, Color color, Vector2 positionOffset, bool overrideFrame = false)
	{
		Tile tile = Main.tile[i, j];
		int frameX = tile.TileFrameX;
		int frameY = tile.TileFrameY;

		if (overrideFrame)
		{
			frameX = 0;
			frameY = 0;
		}

		int width = 16;
		int height = 16;
		var location = new Vector2(i * 16, j * 16);
		Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);
		Vector2 offsets = -Main.screenPosition + zero + positionOffset;
		Vector2 drawLoc = location + offsets;

		if (tile.Slope == 0 && !tile.IsHalfBlock || Main.tileSolid[tile.TileType] && Main.tileSolidTop[tile.TileType]) //second one should be for platforms
		{
			Main.spriteBatch.Draw(texture, drawLoc, new Rectangle(frameX, frameY, width, height), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}
		else if (tile.IsHalfBlock)
		{
			Main.spriteBatch.Draw(texture, new Vector2(drawLoc.X, drawLoc.Y + 8), new Rectangle(frameX, frameY, width, 8), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
		}
		else
		{
			SlopeType b = tile.Slope;
			Rectangle frame;
			Vector2 drawPos;

			if (b is SlopeType.SlopeDownLeft or SlopeType.SlopeDownRight)
			{
				int length;
				int height2;

				for (int a = 0; a < 8; ++a)
				{
					if (b == SlopeType.SlopeDownRight)
					{
						length = 16 - a * 2 - 2;
						height2 = 14 - a * 2;
					}
					else
					{
						length = a * 2;
						height2 = 14 - length;
					}

					frame = new Rectangle(frameX + length, frameY, 2, height2);
					drawPos = new Vector2(i * 16 + length, j * 16 + a * 2) + offsets;
					Main.spriteBatch.Draw(texture, drawPos, frame, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
				}

				frame = new Rectangle(frameX, frameY + 14, 16, 2);
				drawPos = new Vector2(i * 16, j * 16 + 14) + offsets;
				Main.spriteBatch.Draw(texture, drawPos, frame, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
			}
			else
			{
				int length;
				int height2;

				for (int a = 0; a < 8; ++a)
				{
					if (b == SlopeType.SlopeUpLeft)
					{
						length = a * 2;
						height2 = 16 - length;
					}
					else
					{
						length = 16 - a * 2 - 2;
						height2 = 16 - a * 2;
					}

					frame = new Rectangle(frameX + length, frameY + 16 - height2, 2, height2);
					drawPos = new Vector2(i * 16 + length, j * 16) + offsets;
					Main.spriteBatch.Draw(texture, drawPos, frame, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
				}

				drawPos = new Vector2(i * 16, j * 16) + offsets;
				frame = new Rectangle(frameX, frameY, 16, 2);
				Main.spriteBatch.Draw(texture, drawPos, frame, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
			}
		}
	}
}
