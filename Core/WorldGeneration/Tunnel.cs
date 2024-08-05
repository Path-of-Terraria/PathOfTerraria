using System.Collections.Generic;

namespace PathOfTerraria.Core.WorldGeneration;

internal static class Tunnel
{
	public static Vector2[] GeneratePoints(Vector2[] points, int splineCount, float equidistantSpacing)
	{
		points = AddVariationToPoints(points);
		Vector2[] results = Spline.InterpolateXY(points, splineCount);
		return CreateEquidistantSet(results, equidistantSpacing);
	}

	public static Vector2[] AddVariationToPoints(Vector2[] points)
	{
		List<Vector2> newPoints = [];

		for (int i = 0; i < points.Length; i++)
		{
			Vector2 item = points[i];
			newPoints.Add(item);

			if (i == points.Length - 1)
			{
				continue;
			}

			if (i < points.Length - 1 && WorldGen.genRand.NextBool())
			{
				var startLerp = Vector2.Lerp(item, points[i + 1], WorldGen.genRand.NextFloat(0.3f, 0.7f));
				startLerp += item.DirectionTo(points[i + 1]).RotatedBy(MathHelper.Pi * (WorldGen.genRand.NextBool() ? -1 : 1)).RotatedByRandom(0.1f)
					* WorldGen.genRand.NextFloat(10, 20);
				newPoints.Add(startLerp);
			}
			else
			{
				const int Variance = 40;

				newPoints.Add(item + new Vector2(WorldGen.genRand.Next(-Variance, Variance), WorldGen.genRand.Next(Variance)));
			}
		}

		return [.. newPoints];
	}

	public static Vector2[] CreateEquidistantSet(Vector2[] results, float distance)
	{
		List<Vector2> points = [];
		Queue<Vector2> remainingPoints = new(results);
		Vector2 start = remainingPoints.Dequeue();
		Vector2 current = start;
		Vector2 next = remainingPoints.Dequeue();
		float factor = 0;

		while (true)
		{
			float dist = current.Distance(next);

			while (true)
			{
				points.Add(Vector2.Lerp(start, next, factor));
				factor += MathF.Min(1, distance / dist);

				if (factor > 1f)
				{
					break;
				}
			}

			if (remainingPoints.Count == 0)
			{
				return [.. points];
			}

			start = next;
			next = remainingPoints.Dequeue();
			factor--;
		}
	}
}
