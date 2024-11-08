using System.Collections.Generic;
using Terraria.ID;
using Terraria.Utilities;

namespace PathOfTerraria.Common.World.Generation;

internal static class GenPlacement
{
	public const int MossMarker = -1;

	/// <summary>
	/// Valid tile ids per stalactite variant set.
	/// </summary>
	public static Dictionary<int, HashSet<int>> ValidStalactiteAnchorsByVariant = new()
	{
		{ 0, [147, 161, 163, 164, 200] },
		{ 1, [1, MossMarker, 117, 203]},
		{ 5, [25] }
	};

	/// <summary>
	/// Places a stalactite easily, because <see cref="WorldGen.PlaceUncheckedStalactite(int, int, bool, int, bool)"/> sucks.<br/>
	/// Stalactites can only be placed on certain tiles, dictated by <see cref="ValidStalactiteAnchorsByVariant"/>, that aren't sloped and have open space below them.<br/><br/>
	/// <paramref name="baseVariant"/> values are as follows:<br/>
	/// <c>0</c>: Ice (no stalagmite variants)<br/>
	/// <c>1</c>: Stone<br/>
	/// <c>2</c>: Webbed (no small or stalagmite variants)<br/>
	/// <c>3</c>: Honey (only small variant)<br/>
	/// <c>4</c>: Hallowed<br/>
	/// <c>5</c>: Corrupt<br/>
	/// <c>6</c>: Crimson<br/>
	/// <c>7</c>: Desert<br/>
	/// <c>8</c>: Granite<br/>
	/// <c>9</c>: Marble<br/>
	/// <c>10</c>: Hallowed Ice (no stalagmite variants)<br/>
	/// <c>11</c>: Corrupt Ice (no stalagmite variants)<br/>
	/// <c>12</c>: Crimson Ice (no stalagmite variants)<br/>
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="isSmall"></param>
	/// <param name="baseVariant">
	/// Base variant for the tile.<br/>
	/// Values are as follows:<br/>
	/// <c>0</c>: Ice (no stalagmite variants)<br/>
	/// <c>1</c>: Stone<br/>
	/// <c>2</c>: Webbed (no small or stalagmite variants)<br/>
	/// <c>3</c>: Honey (only small variant)<br/>
	/// <c>4</c>: Hallowed<br/>
	/// <c>5</c>: Corrupt<br/>
	/// <c>6</c>: Crimson<br/>
	/// <c>7</c>: Desert<br/>
	/// <c>8</c>: Granite<br/>
	/// <c>9</c>: Marble<br/>
	/// <c>10</c>: Hallowed Ice (no stalagmite variants)<br/>
	/// <c>11</c>: Corrupt Ice (no stalagmite variants)<br/>
	/// <c>12</c>: Crimson Ice (no stalagmite variants)<br/>
	/// </param>
	/// <param name="random"></param>
	/// <returns></returns>
	public static bool PlaceStalactite(int x, int y, bool isSmall, int baseVariant, UnifiedRandom random = null)
	{
		ushort type = TileID.Stalactite;
		int variant = baseVariant * 3;
		short frameX = (short)((random ?? Main.rand).Next(variant, variant + 3) * 18);
		Tile anchor = Main.tile[x, y - 1];
		Tile tile = Main.tile[x, y];
		HashSet<int> anchorTypes = ValidStalactiteAnchorsByVariant[baseVariant];

		if (InvalidStalactiteAnchor(anchor, baseVariant))
		{
			return false;
		}

		if (!isSmall)
		{
			tile.TileType = type;
			tile.HasTile = true;
			tile.TileFrameX = frameX;
			tile.TileFrameY = 0;

			Tile tileBelow = Main.tile[x, y + 1];
			tileBelow.TileType = type;
			tileBelow.HasTile = true;
			tileBelow.TileFrameX = frameX;
			tileBelow.TileFrameY = 18;
		}
		else
		{
			tile.TileType = type;
			tile.HasTile = true;
			tile.TileFrameX = frameX;
			tile.TileFrameY = 72;
		}

		return true;
	}

	/// <inheritdoc cref="PlaceStalactite(int, int, bool, int, UnifiedRandom)"/>
	public static bool PlaceStalagmite(int x, int y, bool isSmall, int baseVariant, UnifiedRandom random = null)
	{
		ushort type = TileID.Stalactite;
		int variant = baseVariant * 3;
		short frameX = (short)((random ?? Main.rand).Next(variant, variant + 3) * 18);
		Tile anchor = Main.tile[x, y + 1];
		Tile tile = Main.tile[x, y];
		
		if (InvalidStalactiteAnchor(anchor, baseVariant))
		{
			return false;
		}

		if (isSmall)
		{
			tile.TileType = type;
			tile.HasTile = true;
			tile.TileFrameX = frameX;
			tile.TileFrameY = 90;
		}
		else
		{
			Tile tileAbove = Main.tile[x, y - 1];
			tileAbove.TileType = type;
			tileAbove.HasTile = true;
			tileAbove.TileFrameX = frameX;
			tileAbove.TileFrameY = 36;

			tile.TileType = type;
			tile.HasTile = true;
			tile.TileFrameX = frameX;
			tile.TileFrameY = 54;
		}

		return true;
	}

	/// <summary>
	/// Determines if a tile is invalid for stalagm/ctite placement.
	/// </summary>
	/// <param name="anchor">Tile to anchor to.</param>
	/// <param name="baseVariant">Variant type.</param>
	/// <returns>If the given variant can be placed on the given anchor.</returns>
	public static bool InvalidStalactiteAnchor(Tile anchor, int baseVariant)
	{
		HashSet<int> anchorTypes = ValidStalactiteAnchorsByVariant[baseVariant];

		if (!anchor.HasTile || !anchorTypes.Contains(anchor.TileType) || anchor.Slope != SlopeType.Solid)
		{
			bool doReturn = true;

			if (anchorTypes.Contains(MossMarker) && Main.tileMoss[anchor.TileType])
			{
				doReturn = false;
			}

			if (doReturn)
			{
				return true;
			}
		}

		return false;
	}
}
