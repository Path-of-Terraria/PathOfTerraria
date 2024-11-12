using PathOfTerraria.Common.World.Generation;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Utilities;

namespace PathOfTerraria.Common.Systems.RealtimeGen;

/// <summary>
/// Helper methods for realtime generation.
/// </summary>
public static class RealtimeSteps
{
	/// <summary>
	/// Wraps around <see cref="WorldGen.PlaceTile(int, int, int, bool, bool, int, int)"/>. Uses default return value
	/// for PlaceTile to determine what the step returns.
	/// </summary>
	/// <param name="x">X position.</param>
	/// <param name="y">Y position.</param>
	/// <param name="type">Tile type to place.</param>
	/// <returns>A realtime step that places a tile.</returns>
	public static RealtimeStep PlaceTile(int x, int y, int type)
	{
		return new RealtimeStep((i, j) =>
		{
			bool value = WorldGen.PlaceTile(i, j, type);

			if (value && Main.netMode != NetmodeID.SinglePlayer) 
			{
				NetMessage.SendTileSquare(-1, i, j);
			}

			return value;
		}, new Point16(x, y));
	}

	/// <summary>
	/// Wraps around <see cref="WorldGen.KillTile(int, int, bool, bool, bool)"/>.<br/>
	/// If there is a tile at <paramref name="x"/>, <paramref name="y"/>, the step returns true.
	/// </summary>
	/// <param name="x">X position.</param>
	/// <param name="y">Y position.</param>
	/// <returns>A realtime step that kills a tile.</returns>
	public static RealtimeStep KillTile(int x, int y)
	{
		return new RealtimeStep((i, j) =>
		{
			WorldGen.KillTile(i, j);
			bool value = !Main.tile[i, j].HasTile;

			if (value && Main.netMode != NetmodeID.SinglePlayer)
			{
				NetMessage.SendTileSquare(-1, i, j);
			}

			return value;
		}, new Point16(x, y));
	}

	/// <summary>
	/// Wraps around <see cref="Tile.SmoothSlope(int, int, bool, bool)"/>.<br/>
	/// The step returns true by default, unless <paramref name="quickSkip"/> is true.
	/// </summary>
	/// <param name="x">X position.</param>
	/// <param name="y">Y position.</param>
	/// <param name="quickSkip">If true, this step will return false for quicker placement.</param>
	/// <returns>A realtime step that slopes a tile.</returns>
	public static RealtimeStep SmoothSlope(int x, int y, bool quickSkip = false)
	{
		return new RealtimeStep((i, j) =>
		{
			Tile.SmoothSlope(i, j);
			
			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				NetMessage.SendTileSquare(-1, i, j);
			}

			return !quickSkip;
		}, new Point16(x, y));
	}

	/// <summary>
	/// Wraps around <see cref="WorldGen.PlaceWall(int, int, int, bool)"/>.<br/>
	/// The step returns if the wall successfully placed.
	/// </summary>
	/// <param name="x">X position.</param>
	/// <param name="y">Y position.</param>
	/// <param name="wall">Wall ID to place.</param>
	/// <param name="quickSkip">If true, this step will return false for quicker placement.</param>
	/// <returns>A realtime step that places a tile.</returns>
	public static RealtimeStep PlaceWall(int x, int y, int wall, bool quickSkip = false)
	{
		return new RealtimeStep((i, j) =>
		{
			WorldGen.PlaceWall(i, j, wall);

			if (Main.netMode != NetmodeID.SinglePlayer && Main.tile[i, j].WallType == wall)
			{
				NetMessage.SendTileSquare(-1, i, j);
			}

			return !quickSkip || Main.tile[i, j].WallType == wall;
		}, new Point16(x, y));
	}

	/// <summary>
	/// Wraps around <see cref="GenPlacement.PlaceStalactite(int, int, bool, int, UnifiedRandom)"/>.<br/>
	/// The step returns if the stalactite was placed properly.
	/// </summary>
	/// <param name="x">X position.</param>
	/// <param name="y">Y position.</param>
	/// <param name="preferSmall">If true, the stalactite will be a 1x1 stalactite.</param>
	/// <param name="baseVariant"><inheritdoc cref="GenPlacement.PlaceStalactite(int, int, bool, int, UnifiedRandom)"/></param>
	/// <param name="random">Random to use when choosing the 3 variants of each stalactite. Defaults to <see cref="Main.rand"/>.</param>
	/// <param name="reframeSquare">
	/// If true, reframes the square around the base of the stalactite. 
	/// Usually, this makes the stalactite look nicer as the tile above/below them merges nicely.
	/// </param>
	/// <returns>A realtime step that places a stalactite.</returns>
	public static RealtimeStep PlaceStalactite(int x, int y, bool preferSmall, int baseVariant, UnifiedRandom random = null, bool reframeSquare = true)
	{
		return new RealtimeStep((i, j) =>
		{
			GenPlacement.PlaceStalactite(i, j, preferSmall, baseVariant, random);

			if (reframeSquare)
			{
				WorldGen.SquareTileFrame(i, j, true);
			}

			bool isStalactite = Main.tile[i, j].TileType == TileID.Stalactite && Main.tile[i, j].HasTile;

			if (Main.netMode != NetmodeID.SinglePlayer && isStalactite)
			{
				NetMessage.SendTileSquare(-1, i, j - 1, 3);
			}

			return isStalactite;
		}, new Point16(x, y));
	}
}
