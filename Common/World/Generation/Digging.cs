namespace PathOfTerraria.Common.World.Generation;

/// <summary>
/// Stores a bunch of digging tools.
/// </summary>
internal static class Digging
{
	/// <summary>
	/// Digs a perfectly circular space into tiles.
	/// </summary>
	/// <param name="pos">Center of the circle, in tile coordinates..</param>
	/// <param name="size">Radius of the circle.</param>
	public static void CircleOpening(Vector2 pos, float size)
	{
		for (int i = (int)(pos.X - size); i < (int)pos.X + size; ++i)
		{
			for (int j = (int)(pos.Y - size); j < (int)pos.Y + size; ++j)
			{
				if (Vector2.DistanceSquared(pos, new Vector2(i, j)) < size * size)
				{
					WorldGen.KillTile(i, j);
				}
			}
		}
	}

	/// <summary>
	/// Digs a perfectly circular space into walls.
	/// </summary>
	/// <param name="pos">Center of the circle.</param>
	/// <param name="size">Radius of the circle.</param>
	public static void WallCircleOpening(Vector2 pos, float size)
	{
		for (int i = (int)(pos.X - size); i < (int)pos.X + size; ++i)
		{
			for (int j = (int)(pos.Y - size); j < (int)pos.Y + size; ++j)
			{
				if (Vector2.DistanceSquared(pos, new Vector2(i, j)) < size * size)
				{
					WorldGen.KillWall(i, j);
				}
			}
		}
	}
}
