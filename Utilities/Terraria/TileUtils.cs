using System.Runtime.CompilerServices;
using PathOfTerraria.Utilities.Xna;

namespace PathOfTerraria.Utilities.Terraria;

/// <summary> Provides basic tile utilities.  </summary>
internal static class TileUtils
{
	/// <summary> The size of a tile in pixel units. </summary>
	public const int TileSizeInPixels = 16;
	/// <summary> The size of a pixel in tile units. </summary>
	public const float PixelSizeInUnits = 1f / TileSizeInPixels;

	// Tilemap utilities.

	/// <summary> Attempts to fit an AABB rectangle into a given point. Avoid calling this in loops. </summary>
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	public static bool TryFitRectangleIntoTilemap(Vector2Int point, Vector2Int size, out Vector2Int adjustedPoint)
	{
		// Inclusive bounds.
		int offsetX1 = -(size.X - 1);
		int offsetX2 = 0;
		int offsetY1 = -(size.Y - 1);
		int offsetY2 = 0;

		for (int offsetX = offsetX1; offsetX <= offsetX2; offsetX++)
		{
			for (int offsetY = offsetY1; offsetY <= offsetY2; offsetY++)
			{
				// Exclusive bounds.
				(int checkX1, int checkY1) = (point.X + offsetX, point.Y + offsetY);
				(int checkX2, int checkY2) = (checkX1 + size.X, checkY1 + size.Y);

				if (checkX1 < 0 | checkY1 < 0 | checkX2 >= Main.maxTilesX | checkY2 >= Main.maxTilesY)
				{
					continue;
				}

				for (int checkX = checkX1; checkX < checkX2; checkX++)
				{
					for (int checkY = checkY1; checkY < checkY2; checkY++)
					{
						Tile tile = Main.tile[checkX, checkY];
						if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType])
						{
							goto ContinueAttempts;
						}
					}
				}

				adjustedPoint = new Vector2Int(checkX1, checkY1);
				return true;

				ContinueAttempts:;
			}
		}

		adjustedPoint = default;
		return false;
	}
}