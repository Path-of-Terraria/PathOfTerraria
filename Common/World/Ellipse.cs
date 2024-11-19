using System.Collections.Generic;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.World;

internal static class Ellipse
{
	public static void Outline(Action<int, int> action, Point16 first, Point16 last)
	{
		List<Point16> throwaway = [];
		Outline(action, first, last, ref throwaway);
	}

	/// <summary>
	/// Creates the outline of an ellipse.
	/// </summary>
	public static void Outline(Action<int, int> action, Point16 first, Point16 last, ref List<Point16> points)
	{
		Point topLeft = new(Math.Min(first.X, last.X), Math.Min(first.Y, last.Y));
		Point bottomRight = new(Math.Max(first.X, last.X), Math.Max(first.Y, last.Y));
		var center = Vector2.Lerp(topLeft.ToVector2(), bottomRight.ToVector2(), 0.5f);

		int width = bottomRight.X - topLeft.X;
		int height = bottomRight.Y - topLeft.Y;
		float perimeter = MathHelper.Pi * (3 * (width + height) - MathF.Sqrt((3 * width + height) * (width + 3 * height))); //Approximate perimeter

		if (perimeter == 0)
		{
			return;
		}

		Point16 lastPlace = new();
		float interval = MathHelper.TwoPi / (perimeter * 1.5f);

		for (float repeats = 0; repeats < MathHelper.TwoPi; repeats += interval)
		{
			int x = (int)(width * MathF.Cos(repeats)) + (int)center.X;
			int y = (int)(height * MathF.Sin(repeats)) + (int)center.Y;

			Vector2 vector = lastPlace.ToVector2() - new Vector2(x, y);

			if (vector.RotatedBy(MathHelper.PiOver2).Length() >= 1)
			{
				action(x, y);
				points.Add(new Point16(x, y));
			}
		}
	}

	public static void AngledOutline(Action<int, int> action, Point16 first, Point16 last, float angle)
	{
		List<Point16> points = [];	
		AngledOutline(action, first, last, angle, ref points);
	}

	public static void AngledOutline(Action<int, int> action, Point16 first, Point16 last, float angle, ref List<Point16> points)
	{
		Point topLeft = new(Math.Min(first.X, last.X), Math.Min(first.Y, last.Y));
		Point bottomRight = new(Math.Max(first.X, last.X), Math.Max(first.Y, last.Y));
		var center = Vector2.Lerp(topLeft.ToVector2(), bottomRight.ToVector2(), 0.5f);

		int width = bottomRight.X - topLeft.X;
		int height = bottomRight.Y - topLeft.Y;
		float perimeter = MathHelper.Pi * (3 * (width + height) - MathF.Sqrt((3 * width + height) * (width + 3 * height))); //Approximate perimeter

		if (perimeter == 0)
		{
			return;
		}

		Point16 lastPlace = new();
		float interval = MathHelper.TwoPi / (perimeter * 1.5f);

		for (float repeats = 0; repeats < MathHelper.TwoPi; repeats += interval)
		{
			int x = (int)(width * MathF.Cos(repeats)) + (int)center.X;
			int y = (int)(height * MathF.Sin(repeats)) + (int)center.Y;

			Vector2 vector = lastPlace.ToVector2() - new Vector2(x, y);

			if (vector.RotatedBy(MathHelper.PiOver2).Length() >= 1)
			{
				Vector2 adj = new(x - first.X, y - first.Y);
				adj = adj.RotatedBy(angle) + first.ToVector2();
				x = (int)adj.X;
				y = (int)adj.Y;
				action(x, y);
				points.Add(new Point16(x, y));
			}
		}
	}

	public static void Fill(Action<int, int> action, Point16 origin, float widthSize, float heightSize, float angle, ref List<Point16> points, Func<int, int, float> offset)
	{
		float size = MathF.Max(widthSize, heightSize);
		float dist = size / 3.5f;
		size *= 2f;

		for (int i = origin.X - (int)size; i < origin.X + size; ++i)
		{
			for (int j = origin.Y - (int)size; j < origin.Y + size; ++j)
			{
				if (!WorldGen.InWorld(i, j, 10))
				{
					continue;
				}

				int x = i - origin.X;
				int y = j - origin.Y;
				Vector2 offsetPos = new Vector2(x, y).RotatedBy(angle) + origin.ToVector2();

				float distance = MathF.Sqrt(MathF.Pow(offsetPos.X - origin.X, 2) * (heightSize / widthSize) + MathF.Pow(offsetPos.Y - origin.Y, 2) * (widthSize / heightSize));
				float noiseValue = offset is null ? 0 : offset(i, j);

				if (distance < dist + noiseValue)
				{
					action(i, j);
					points.Add(new(i, j));
				}
			}
		}
	}
}
