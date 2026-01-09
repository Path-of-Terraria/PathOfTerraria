using System.Runtime.CompilerServices;

namespace PathOfTerraria.Common.World.Utilities;

internal static class WorldUtilities
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool SolidTile(int i, int j)
	{
		return SolidTile(Main.tile[i, j]);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool SolidTile(Tile tile)
	{
		return Main.tileSolid[tile.TileType] && tile.HasTile;
	}

	/// <summary>
	/// Determines if the tile is standing without "anchors", i.e. it is midair by itself.
	/// </summary>
	public static bool TileOrphaned(int i, int j)
	{
		return !SolidTile(i, j - 1) && !SolidTile(i, j + 1) && !SolidTile(i - 1, j) && !SolidTile(i + 1, j);
	}
}
