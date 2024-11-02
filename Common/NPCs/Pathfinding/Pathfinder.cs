using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.NPCs.Pathfinding;

internal class Pathfinder
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

	public bool HasPath { get; private set; }
	
	public ReadOnlyCollection<FoundPoint> Path { get; private set; }

	readonly Dictionary<Point16, WorkingPoint> found = [];
	readonly PriorityQueue<Point16, float> frontier = new();
	readonly List<FoundPoint> path = [];

	public bool DrawPath(Point16 start, Point16 end)
	{
		path.Clear();
		found.Clear();
		frontier.Clear();

		if (Solid(start) || Solid(end))
		{
			return false;
		}

		AddPoint(end, start, Direction.None, 0);

		while (frontier.Count > 0)
		{
			Point16 pos = frontier.Dequeue();

			if (pos == end)
			{
				BuildPath(start, end);
				HasPath = true;
				return true;
			}

			AddSurrounds(end, pos, found[pos].Weight);
		}

		return false;
	}

	private void BuildPath(Point16 start, Point16 end)
	{
		Point16 pos = end;

		while (pos != start)
		{
			Direction dir = found[pos].Direction;
			path.Add(new FoundPoint(pos, dir));

			Point16 direction = ToVector(dir);
			pos = new Point16(pos.X + direction.X, pos.Y + direction.Y);
		}

		Path = new(path);
	}

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

	public static bool Solid(Point16 position)
	{
		return WorldGen.SolidOrSlopedTile(position.X, position.Y);
	}

	public bool SolidOrInvalid(Point16 position)
	{
		return Solid(position) || found.ContainsKey(position);
	}

	private void AddSurrounds(Point16 end, Point16 pos, float currentWeight)
	{
		if (!SolidOrInvalid(new Point16(pos.X, pos.Y + 1)))
		{
			AddPoint(end, new Point16(pos.X, pos.Y + 1), Direction.Above, currentWeight);
		}

		if (!SolidOrInvalid(new Point16(pos.X, pos.Y - 1)))
		{
			AddPoint(end, new Point16(pos.X, pos.Y - 1), Direction.Below, currentWeight);
		}

		if (!SolidOrInvalid(new Point16(pos.X + 1, pos.Y)))
		{
			AddPoint(end, new Point16(pos.X + 1, pos.Y), Direction.Left, currentWeight);
		}

		if (!SolidOrInvalid(new Point16(pos.X - 1, pos.Y)))
		{
			AddPoint(end, new Point16(pos.X - 1, pos.Y), Direction.Right, currentWeight);
		}
	}

	private void AddPoint(Point16 end, Point16 position, Direction direction, float currentWeight)
	{
		found.Add(position, new WorkingPoint(direction, currentWeight));
		frontier.Enqueue(position, currentWeight + SquaredDistance(end, position));
	}

	private static float Distance(Point16 start, Point16 position)
	{
		return MathF.Sqrt(MathF.Pow(position.Y - start.Y, 2) + MathF.Pow(position.X - start.X, 2));
	}

	private static float SquaredDistance(Point16 start, Point16 position)
	{
		return MathF.Pow(position.Y - start.Y, 2) + MathF.Pow(position.X - start.X, 2);
	}
}
