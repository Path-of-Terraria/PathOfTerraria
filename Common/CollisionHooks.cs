using Terraria.ID;

namespace PathOfTerraria.Common;

/// <summary>
/// Currently unused, but has code that may be useful in the future.
/// </summary>
internal class CollisionHooks
{
	/// <summary>
	/// Tries to properly deterimine if the given hitbox has tile collision, taking into account sloped tiled.
	/// </summary>
	/// <param name="pos">World position of the hitbox.</param>
	/// <param name="width">Width of the hitbox.</param>
	/// <param name="height">Height of the hitbox.</param>
	/// <returns>If collision was detected.</returns>
	/// <exception cref="ArgumentException"/>
	public static bool ActualSolidCollision(Vector2 pos, int width, int height)
	{
		var hitbox = new Rectangle((int)pos.X, (int)pos.Y, width, height);
		Point min = hitbox.TopLeft().ToTileCoordinates();
		Point max = hitbox.BottomRight().ToTileCoordinates();

		for (int i = min.X; i < max.X; ++i)
		{
			for (int j = min.Y; j < max.Y; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (!tile.HasTile || !Main.tileSolid[tile.TileType] || tile.IsActuated)
				{
					continue;
				}

				if (tile.Slope == SlopeType.Solid)
				{
					var other = new Rectangle(i * 16, j * 16 + (tile.IsHalfBlock ? 8 : 0), 16, tile.IsHalfBlock ? 8 : 16);

					if (hitbox.Intersects(other))
					{
						return true;
					}
				}
				else
				{
					Vector2[] trianglePoints = tile.Slope switch
					{
						SlopeType.SlopeUpRight => [new Vector2(i, j), new Vector2(i + 1, j), new Vector2(i + 1, j + 1)],
						SlopeType.SlopeDownRight => [new Vector2(i + 1, j), new Vector2(i + 1, j + 1), new Vector2(i, j + 1)],
						SlopeType.SlopeDownLeft => [new Vector2(i, j), new Vector2(i + 1, j + 1), new Vector2(i, j + 1)],
						SlopeType.SlopeUpLeft => [new Vector2(i, j), new Vector2(i + 1, j), new Vector2(i, j + 1)],
						_ => throw new ArgumentException("How is this possible")
					};

					if (TriangleSquareCollision(trianglePoints, hitbox))
					{
						return true;
					}
				}
			}
		}

		return false;
	}

	public static bool TriangleSquareCollision(Vector2[] trianglePoints, Rectangle hitbox)
	{
		Vector2 center = Vector2.Zero;

		for (int k = 0; k < 3; ++k)
		{
			Vector2 current = trianglePoints[k] * 16;

			if (hitbox.Contains(trianglePoints[k].ToPoint()))
			{
				return true;
			}

			Vector2 next = (k == 2 ? trianglePoints[0] : trianglePoints[k + 1]) * 16;

			if (Collision.CheckAABBvLineCollision(hitbox.Location.ToVector2(), hitbox.Size(), current, next))
			{
				return true;
			}

			center += current;
		}

		center /= 3;

		if (hitbox.Contains(center.ToPoint()))
		{
			return true;
		}

		return false;
	}

	/// <summary>
	/// Doesn't work, will need a rewrite.
	/// </summary>
	public static bool CheckTriangleLine(Vector2 current, Vector2 next, Rectangle hitbox, float minY, float maxY)
	{
		float slope = (next.Y - current.Y) / (next.X - current.X);
		float intercept = current.Y - slope * current.X;

		float topIntersection = slope * hitbox.Left + intercept;
		float bottomIntersection = slope * hitbox.Right + intercept;

		if (slope <= 0)
		{
			(topIntersection, bottomIntersection) = (bottomIntersection, topIntersection);
		}

		float topOverlap = current.Y > minY ? topIntersection : minY;
		float bottomOverlap = bottomIntersection < maxY ? bottomIntersection : maxY;
		bool valid = topOverlap < bottomOverlap && bottomOverlap < maxY;

		return valid;
	}
}
