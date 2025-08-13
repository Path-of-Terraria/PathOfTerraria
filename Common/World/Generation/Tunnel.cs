using System.Collections.Generic;
using System.Linq;

namespace PathOfTerraria.Common.World.Generation;

internal static class Tunnel
{
	/// <summary>
	/// Generates a list of points that creates a cohesive, singular, smooth tunnel.
	/// </summary>
	/// <param name="points">Base points to generate between.</param>
	/// <param name="splineCount">The amount of splines to generate; that is, the amount of "control points" on the line. More = more smooth, but may add weird loops.</param>
	/// <param name="equidistantSpacing">How close each point should be, in singular units (usually tiles).</param>
	/// <param name="variationMultiplier">
	/// A randomized default variation algorithm's strength. 
	/// Offsets each point before creating the equidistant set. 0 skips variation entirely.
	/// </param>
	/// <returns>The equidistant, smooth and singular curved tunnel.</returns>
	public static Vector2[] GeneratePoints(Vector2[] points, int splineCount, float equidistantSpacing, float variationMultiplier = 1f)
	{
		if (variationMultiplier != 0)
		{
			points = AddVariationToPoints(points, variationMultiplier);
		}

		Vector2[] results = Spline.InterpolateXY(points, splineCount);
		return CreateEquidistantSet(results, equidistantSpacing, true);
	}

	/// <summary>
	/// Generates a list of points that creates a cohesive, singular, smooth tunnel. This overload returns the 'base' spline before the equidistant set for debugging.
	/// </summary>
	/// <param name="points">Base points to generate between.</param>
	/// <param name="baseSpline">The default spline made by <see cref="Spline.InterpolateXY(Vector2[], int)"/>.</param>
	/// <param name="splineCount">The amount of splines to generate; that is, the amount of "control points" on the line. More = more smooth, but may add weird loops.</param>
	/// <param name="equidistantSpacing">How close each point should be, in singular units (usually tiles).</param>
	/// <param name="variationMultiplier">
	/// A randomized default variation algorithm's strength. 
	/// Offsets each point before creating the equidistant set. 0 skips variation entirely.
	/// </param>
	/// <returns>The equidistant, smooth and singular curved tunnel.</returns>
	public static Vector2[] GeneratePoints(Vector2[] points, out Vector2[] baseSpline, int splineCount, float equidistantSpacing, float variationMultiplier = 1f)
	{
		if (variationMultiplier != 0)
		{
			points = AddVariationToPoints(points, variationMultiplier);
		}

		baseSpline = Spline.InterpolateXY(points, splineCount);
		return CreateEquidistantSet(baseSpline, equidistantSpacing, true);
	}

	public static Vector2[] AddVariationToPoints(Vector2[] points, float variationMultiplier = 1f)
	{
		List<Vector2> newPoints = [];

		for (int i = 0; i < points.Length; i++)
		{
			Vector2 item = points[i];
			newPoints.Add(item);

			if (i == points.Length - 1 || item == points[i + 1])
			{
				continue;
			}

			if (i < points.Length - 1 && WorldGen.genRand.NextBool())
			{
				var startLerp = Vector2.Lerp(item, points[i + 1], WorldGen.genRand.NextFloat(0.3f, 0.7f));
				startLerp += item.DirectionTo(points[i + 1]).RotatedBy(MathHelper.Pi * (WorldGen.genRand.NextBool() ? -1 : 1)).RotatedByRandom(0.1f)
					* WorldGen.genRand.NextFloat(10, 20) * variationMultiplier;
				newPoints.Add(startLerp);
			}
			else
			{
				const int Variance = 40;

				newPoints.Add(item + new Vector2(WorldGen.genRand.Next(-Variance, Variance), WorldGen.genRand.Next(Variance)) * variationMultiplier);
			}
		}

		HashSet<Vector2> uniques = [];

		foreach (Vector2 pos in newPoints)
		{
			uniques.Add(pos);
		}

		return [.. uniques];
	}

	public static Vector2[] CreateEquidistantSet(Vector2[] results, float distance, bool resetOnRefactor = false)
	{
		List<Vector2> points = [];
		Queue<Vector2> remainingPoints = new(results);
		Vector2 current = remainingPoints.Dequeue();
		Vector2 next = remainingPoints.Dequeue();
		float factor = 0;

		if (results.Any(x => x.HasNaNs()))
		{
			return results;
		}

		while (true)
		{
			float dist = current.Distance(next);

			while (true)
			{
				points.Add(Vector2.Lerp(current, next, factor));
				factor += distance / dist;

				if (factor > 1f || float.IsNaN(dist))
				{
					break;
				}
			}

			if (remainingPoints.Count == 0)
			{
				return [.. points];
			}

			if (resetOnRefactor)
			{
				if (factor > 1f)
				{
					if (remainingPoints.Count == 0)
					{
						return [.. points];
					}

					current = next;
					next = remainingPoints.Dequeue();
					factor--;

					factor *= dist / current.Distance(next);
				}

				factor = 0;
			}
			else
			{
				while (factor > 1f)
				{
					if (remainingPoints.Count == 0)
					{
						return [.. points];
					}

					current = next;
					next = remainingPoints.Dequeue();
					factor--;

					factor *= dist / current.Distance(next);
				}
			}
		}
	}
}
