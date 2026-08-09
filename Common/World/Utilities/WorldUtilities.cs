using System.Runtime.CompilerServices;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.World.Utilities;

internal static class WorldUtilities
{
	public static bool SolidTile(int i, int j) { return SolidTile(Main.tile[i, j]); }
	public static bool SolidTile(Point16 point) { return SolidTile(Main.tile[point]); }
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool SolidTile(Tile tile)
	{
		return Main.tileSolid[tile.TileType] && tile.HasUnactuatedTile;
	}

	/// <inheritdoc cref="SolidOrActuatedTile(Tile)"/>
	public static bool SolidOrActuatedTile(int i, int j) { return SolidOrActuatedTile(Main.tile[i, j]); }
	/// <inheritdoc cref="SolidOrActuatedTile(Tile)"/>
	public static bool SolidOrActuatedTile(Point16 point) { return SolidOrActuatedTile(Main.tile[point]); }
	/// <summary> Checks if a tile is solid, regardless of if its actuated. </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool SolidOrActuatedTile(Tile tile)
	{
		return Main.tileSolid[tile.TileType] && tile.HasTile;
	}

	/// <inheritdoc cref="SolidUnslopedTile(Tile, bool)"/>
	public static bool SolidUnslopedTile(int i, int j, bool noHalfBrick = false) { return SolidUnslopedTile(Main.tile[i, j], noHalfBrick); }
	/// <inheritdoc cref="SolidUnslopedTile(Tile, bool)"/>
	public static bool SolidUnslopedTile(Point16 point, bool noHalfBrick = false) { return SolidUnslopedTile(Main.tile[point], noHalfBrick); }
	/// <summary> Checks if a tile is solid and not sloped. Optionally also checks for half-bricks. </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool SolidUnslopedTile(Tile tile, bool noHalfBrick = false)
	{
		return Main.tileSolid[tile.TileType] && tile.HasTile && tile.Slope == SlopeType.Solid && (noHalfBrick || !tile.IsHalfBlock);
	}

	/// <summary>
	/// Determines if the tile is standing without "anchors", i.e. it is midair by itself. <b>Does not</b> include actuated adjacent tiles.
	/// </summary>
	public static bool TileOrphanedStrict(int i, int j)
	{
		return !SolidTile(i, j - 1) && !SolidTile(i, j + 1) && !SolidTile(i - 1, j) && !SolidTile(i + 1, j);
	}

	/// <summary>
	/// Determines if the tile is standing without "anchors", i.e. it is midair by itself. Includes actuated adjacent tiles.
	/// </summary>
	public static bool TileOrphaned(int i, int j)
	{
		return !SolidOrActuatedTile(i, j - 1) && !SolidOrActuatedTile(i, j + 1) && !SolidOrActuatedTile(i - 1, j) && !SolidOrActuatedTile(i + 1, j);
	}

	/// <summary>
	/// Determines if a tile has any tiles adjacent to it that have no walls.
	/// </summary>
	internal static bool TileExposedToAirWalls(int i, int j, bool diagonal = true)
	{
		const int None = WallID.None;

		if (Main.tile[i, j - 1].WallType == None || Main.tile[i, j + 1].WallType == None || Main.tile[i + 1, j].WallType == None || Main.tile[i - 1, j].WallType == None)
		{
			return true;
		}

		if (!diagonal)
		{
			return false;
		}

		if (Main.tile[i - 1, j - 1].WallType == None || Main.tile[i + 1, j + 1].WallType == None || Main.tile[i + 1, j - 1].WallType == None || Main.tile[i - 1, j + 1].WallType == None)
		{
			return true;
		}

		return false;
	}
}
