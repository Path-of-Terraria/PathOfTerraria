using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.NPCs.Pathfinding;

/// <summary>
/// Contains an individual pathfinder for an entity or object. 
/// Call <see cref="CheckDrawPath(Point16, Point16, Rectangle)"/> to check if a path needs to be [re]drawn.<br/>
/// Pathfinding will be cached for next frame if the current frame could not finish pathfinding. That means pathfinding can and will be split into many frames if needed.<br/>
/// This should not be relevant for all but very complex and large areas.
/// </summary>
/// <param name="refreshTime">
/// How often the path should be refreshed, not including caching. 
/// DO NOTE: If caching is really slow, a path may never be found but will be continually refreshed.<br/>
/// Make sure this works in testing and especially use a reasonable <see cref="checkingRectangle"/>!
/// </param>
internal class Pathfinder(int refreshTime)
{
	public enum Direction : byte
	{
		None,
		Above,
		Below,
		Left,
		Right
	}

	public readonly record struct FoundPoint(Point16 Position, Direction Direction);
	public readonly record struct WorkingPoint(Direction Direction, float Weight);

	/// <summary>
	/// Whether the current pathfinder has a valid <see cref="Path"/> or not.
	/// </summary>
	public bool HasPath { get; private set; }

	/// <summary>
	/// The movement path from the last start, end pair.
	/// </summary>
	public List<FoundPoint> Path { get; private set; } = [];

	public readonly int RefreshTime = refreshTime;

	public int RefreshTimer = 0;

	private readonly Dictionary<Point16, WorkingPoint> found = [];
	private readonly PriorityQueue<Point16, float> frontier = new();
	private (Point16 start, Point16 end) cachedLocations = new();
	private Rectangle checkingRectangle = default;
	private bool cached = false;
	private Vector2 objectSize = default;
	private Vector2 posOffset = default;

	/// <summary>
	/// Draws a path from <paramref name="start"/> to <paramref name="end"/> with A*, setting <see cref="Path"/> if <see cref="HasPath"/> is true.<br/>
	/// Will cache pathfinding over the course of many frames if necessary.<br/>
	/// This should be run every frame.
	/// </summary>
	/// <param name="start">Start point of the search, in tiles.</param>
	/// <param name="end">End point of the search, in tiles.</param>
	/// <param name="checkArea">
	/// The effective calculation area for the pathfinding.<br/>
	/// This could be, for example, a rectangle containing both the start and end as corners inflated to include any reasonable surroundings.<br/>
	/// Use this to optimize checking area and reduce caching and failure times.<br/><br/>
	/// Defaults to a <see cref="GetCheckArea(Point16, Point16, Point16)"/> on <paramref name="start"/>, <paramref name="end"/> fluffed by 200, 180.
	/// </param>
	/// <returns>Whether a new path was found this frame. This includes cached scans finishing.</returns>
	public bool CheckDrawPath(Point16 start, Point16 end, Vector2 objSizeInTiles = default, Rectangle? checkArea = null, Vector2? positionOffset = null)
	{
		// Run only on sp or server...once we get better syncing in
		//if (Main.netMode == NetmodeID.MultiplayerClient)
		//{
		//	return false;
		//}

		RefreshTimer--;

		if (start == end)
		{
			return true;
		}

		checkingRectangle = checkArea ?? GetCheckArea(start, end, new Point16(200, 180));
		objectSize = objSizeInTiles;
		posOffset = positionOffset ?? Vector2.Zero;

		if (RefreshTimer == 0)
		{
			// The timer has reset; stop the old cached result and restart
			ResetState();
		}

		if (!cached && RefreshTimer > 0)
		{
			return false;
		}

		if (!cached)
		{
			Path.Clear();
			found.Clear();
			frontier.Clear();

			AddPoint(end, start, Direction.None, 0);
			RefreshTimer = RefreshTime;
		}
		else
		{
			start = cachedLocations.start;
			end = cachedLocations.end;
		}

		if (objectSize == default ? (Solid(start) || Solid(end)) : (SolidBig(start, objectSize, posOffset) || SolidBig(end, objectSize, posOffset)))
		{
			return false;
		}

		int iterations = 0;

		while (frontier.Count > 0)
		{
			iterations++;

			if (iterations > 7000)
			{
				cached = true;
				cachedLocations = (start, end);
				return false;
			}

			Point16 pos = frontier.Dequeue();

			if (pos.Equals(end))
			{
				BuildPath(start, end);
				ResetState();

				HasPath = true;
				RefreshTimer = RefreshTime;
				return true;
			}

			AddSurrounds(pos, end, found[pos].Weight);
		}

		ResetState();
		return false;
	}

	private void ResetState()
	{
		cached = false;
		found.Clear();
	}

	/// <summary>
	/// Sets <see cref="Path"/>, which follows the path back to the origin point to serve as a movement path.<br/>
	/// This assumes pathfinding is done.
	/// </summary>
	/// <param name="start">Start point.</param>
	/// <param name="end">End point.</param>
	private void BuildPath(Point16 start, Point16 end)
	{
		Point16 pos = end;

		while (pos != start)
		{
			Direction dir = found[pos].Direction;
			Path.Add(new FoundPoint(pos, dir));

			Point16 direction = ToVector(dir);
			pos = new Point16(pos.X + direction.X, pos.Y + direction.Y);
		}

		Path = new(Path);
	}

	/// <summary>
	/// Converts a <see cref="Direction"/> to a vector <see cref="Point16"/> direction value.
	/// </summary>
	/// <param name="direction">Direction to convert.</param>
	/// <returns>Vector representation of the direction.</returns>
	public static Point16 ToVector(Direction direction)
	{
		return direction switch
		{
			Direction.Left => new Point16(-1, 0),
			Direction.Right => new Point16(1, 0),
			Direction.Below => new Point16(0, 1),
			Direction.Above => new Point16(0, -1),
			_ => Point16.Zero
		};
	}

	/// <summary>
	/// Converts a <see cref="Direction"/> to a vector <see cref="Vector2"/> direction value.
	/// </summary>
	/// <param name="direction">Direction to convert.</param>
	/// <returns>Vector2 representation of the direction.</returns>
	public static Vector2 ToVector2(Direction direction)
	{
		return direction switch
		{
			Direction.Left => new Vector2(-1, 0),
			Direction.Right => new Vector2(1, 0),
			Direction.Below => new Vector2(0, 1),
			Direction.Above => new Vector2(0, -1),
			_ => Vector2.Zero
		};
	}

	/// <summary>
	/// Helper method to shorten checks to see if a tile is a validly solid tile. Checks if the tile is solid and not a closed door.
	/// </summary>
	/// <param name="position">Position of the tile.</param>
	/// <returns>If the tile is a solid non-door tile.</returns>
	public static bool Solid(Point16 position)
	{
		ushort tileType = Main.tile[position].TileType;

		return WorldGen.SolidOrSlopedTile(position.X, position.Y) && TileID.Sets.OpenDoorID[tileType] == -1 && tileType != TileID.ClosedDoor;
	}

	/// <summary>
	/// Determines if a tile cannot be added to <see cref="frontier"/>; behaves as obstacles in graph traversal.
	/// </summary>
	/// <param name="position">Position of the tile.</param>
	/// <returns>If the tile is invalid or not.</returns>
	public bool InvalidTile(Point16 position)
	{
		return !checkingRectangle.Contains(position.ToPoint()) || (objectSize == default ? Solid(position) : SolidBig(position, objectSize, posOffset)) 
			|| found.ContainsKey(position);
	}

	/// <summary>
	/// Helper method for checking if a >16x16px object is blocked by tiles.<br/>
	/// NOTE: This counts sloped tiles as full, solid tiles - this isn't perfect.
	/// </summary>
	/// <param name="position">Position, in tile coordinates, of the object.</param>
	/// <param name="objectSize">Hitbox size, in tile coordinates, of the object.</param>
	/// <param name="positionOffset">Offsets the position, such as if you want to origin the position to the original entity's Center instead of position.</param>
	/// <returns></returns>
	private static bool SolidBig(Point16 position, Vector2 objectSize, Vector2 positionOffset)
	{
		Vector2 pos = position.ToWorldCoordinates() + positionOffset;
		return Collision.SolidCollision(pos, (int)(objectSize.X * 16), (int)(objectSize.Y * 16));
	}

	/// <summary>
	/// Adds every valid tile around the given position.
	/// </summary>
	/// <param name="pos">Position of the tile.</param>
	/// <param name="end">Goal position, used for <see cref="AddPoint(Point16, Point16, Direction, float)"/>'s heuristics.</param>
	/// <param name="currentWeight">Current weight of the algorithm.</param>
	private void AddSurrounds(Point16 pos, Point16 end, float currentWeight)
	{
		if (!InvalidTile(new Point16(pos.X, pos.Y + 1)))
		{
			AddPoint(end, new Point16(pos.X, pos.Y + 1), Direction.Above, currentWeight);
		}

		if (!InvalidTile(new Point16(pos.X, pos.Y - 1)))
		{
			AddPoint(end, new Point16(pos.X, pos.Y - 1), Direction.Below, currentWeight);
		}

		if (!InvalidTile(new Point16(pos.X + 1, pos.Y)))
		{
			AddPoint(end, new Point16(pos.X + 1, pos.Y), Direction.Left, currentWeight);
		}

		if (!InvalidTile(new Point16(pos.X - 1, pos.Y)))
		{
			AddPoint(end, new Point16(pos.X - 1, pos.Y), Direction.Right, currentWeight);
		}
	}

	/// <summary>
	/// Adds a point to <see cref="found"/> and <see cref="frontier"/> if it's in-world.
	/// </summary>
	/// <param name="end">End position for the heuristic.</param>
	/// <param name="position">Position of the tile.</param>
	/// <param name="direction">Direction of the new tile.</param>
	/// <param name="currentWeight">Current weight of the new tile.</param>
	private void AddPoint(Point16 end, Point16 position, Direction direction, float currentWeight)
	{
		if (!WorldGen.InWorld(position.X, position.Y, 40))
		{
			return;
		}

		found.Add(position, new WorkingPoint(direction, currentWeight));
		frontier.Enqueue(position, currentWeight + SquaredDistance(end, position));
	}

	/// <summary>
	/// Helper method for distance between two points.
	/// </summary>
	/// <param name="start">Start position.</param>
	/// <param name="position">End position.</param>
	/// <returns>Distance between two points.</returns>
	private static float Distance(Point16 start, Point16 position)
	{
		return MathF.Sqrt(MathF.Pow(position.Y - start.Y, 2) + MathF.Pow(position.X - start.X, 2));
	}

	/// <summary>
	/// Helper method for the squared distance between two points.
	/// </summary>
	/// <param name="start">Start position.</param>
	/// <param name="position">End position.</param>
	/// <returns>Squared distance between two points.</returns>
	private static float SquaredDistance(Point16 start, Point16 position)
	{
		return MathF.Pow(position.Y - start.Y, 2) + MathF.Pow(position.X - start.X, 2);
	}

	/// <summary>
	/// Generates a rectangle that surrounds both <paramref name="start"/> and <paramref name="end"/> and is fluffed by <paramref name="inflationSize"/>.
	/// </summary>
	/// <param name="start">Point one.</param>
	/// <param name="end">Point two.</param>
	/// <param name="inflationSize">Additional fluff for the given rectangle.</param>
	/// <returns>The check area rectangle.</returns>
	public static Rectangle GetCheckArea(Point16 start, Point16 end, Point16 inflationSize)
	{
		int minX = Math.Min(start.X, end.X);
		int minY = Math.Min(start.X, end.Y);
		int maxX = Math.Max(start.X, end.X);
		int maxY = Math.Max(start.X, end.Y);

		Rectangle rect = new(minX, minY, maxX - minX, maxY - minY);
		rect.Inflate(inflationSize.X, inflationSize.Y);
		return rect;
	}

	// NOTE: Both of these should be unused as something isn't working in order to give this the intended effect.
	public void SendPath(BinaryWriter writer)
	{
		if (Path.Count == 0)
		{
			writer.Write(false);
			return;
		}

		writer.Write(true);
		writer.Write(Path[0].Position.X);
		writer.Write(Path[0].Position.Y);
		writer.Write((byte)Path[0].Direction);
		writer.Write((short)Path.Count - 1);

		for (int i = 1; i < Path.Count; ++i)
		{
			writer.Write((byte)Path[i].Direction);
		}
	}

	public void ReadPath(BinaryReader reader)
	{
		bool cont = reader.ReadBoolean();

		if (!cont)
		{
			return;
		}

		short x = reader.ReadInt16();
		short y = reader.ReadInt16();
		var dir = (Direction)reader.ReadByte();
		short count = reader.ReadInt16();
		Point16 last = new(x, y);

		Path.Clear();
		Path.Add(new FoundPoint(last, dir));

		for (int i = 0; i < count; ++i)
		{
			Point16 adj = ToVector(dir);
			last = new Point16(last.X - adj.X, last.Y - adj.Y);
			dir = (Direction)reader.ReadByte();
			Path.Add(new FoundPoint(last, dir));
		}
	}
}
