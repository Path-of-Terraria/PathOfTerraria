using System.Runtime.CompilerServices;

namespace PathOfTerraria.Common.World.Utilities;

internal static class WorldUtils
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
}
