using System.Diagnostics;
using Terraria.UI;

namespace PathOfTerraria.Common.Utilities;

public static class MathUtils
{
	/// <summary> Short-hand for Clamp(value, 0, 1). </summary>
	public static float Clamp01(float value)
	{
		return value <= 0f ? 0f : value >= 1f ? 1f : value;
	}
	/// <summary> Short-hand for Clamp(value, 0, 1). </summary>
	public static double Clamp01(double value)
	{
		return value <= 0.0 ? 0.0 : value >= 1.0 ? 1.0 : value;
	}

	/// <summary> Returns the remainder of a division, compatible with negative numbers. </summary>
	public static float Modulo(float value, float length)
	{
		return value - (float)Math.Floor(value / length) * length;
	}
	/// <summary> Returns the remainder of a division, compatible with negative numbers. </summary>
	public static double Modulo(double value, double length)
	{
		return value - (float)Math.Floor(value / length) * length;
	}
	/// <summary> Returns the remainder of a division, compatible with negative numbers. </summary>
	public static int Modulo(int value, int length)
	{
		int r = value % length;
		return r < 0 ? r + length : r;
	}

	/// <summary> Returns the input with the highest absolute value. </summary>
	public static int MaxAbs(int a, int b)
	{
		return Math.Abs(a) >= Math.Abs(b) ? a : b;
	}
	/// <inheritdoc cref="MaxAbs"/>
	public static float MaxAbs(float a, float b)
	{
		return Math.Abs(a) >= Math.Abs(b) ? a : b;
	}
	/// <summary> Returns the input with the lowest absolute value. </summary>
	public static int MinAbs(int a, int b)
	{
		return Math.Abs(a) <= Math.Abs(b) ? a : b;
	}
	/// <inheritdoc cref="MinAbs"/>
	public static float MinAbs(float a, float b)
	{
		return Math.Abs(a) <= Math.Abs(b) ? a : b;
	}

	// Linear Interpolation

	/// <summary> Approaches <paramref name="goal"/> by adding <paramref name="step"/> to move <paramref name="value"/> towards it. </summary>
	public static float StepTowards(float value, float goal, float step)
	{
		if (goal > value)
		{
			value += step;
			if (value > goal) { return goal; }
		}
		else if (goal < value)
		{
			value -= step;
			if (value < goal) { return goal; }
		}

		return value;
	}

	public static StyleDimension Lerp(StyleDimension from, StyleDimension to, float t)
	{
		return new StyleDimension()
		{
			Pixels = MathHelper.Lerp(from.Pixels, to.Pixels, t),
			Precent = MathHelper.Lerp(from.Precent, to.Precent, t),
		};
	}
	
	public static StyleDimension SmoothStep(StyleDimension from, StyleDimension to, float t)
	{
		return new StyleDimension()
		{
			Pixels = MathHelper.SmoothStep(from.Pixels, to.Pixels, t),
			Precent = MathHelper.SmoothStep(from.Precent, to.Precent, t),
		};
	}

	public static float LerpRadians(float a, float b, float factor)
	{
		float result;
		float diff = b - a;

		if (diff < -MathHelper.Pi) {
			// Lerp upwards past TwoPi
			b += MathHelper.TwoPi;
			result = MathHelper.Lerp(a, b, factor);

			if (result >= MathHelper.TwoPi) {
				result -= MathHelper.TwoPi;
			}
		} else if (diff > MathHelper.Pi) {
			// Lerp downwards past 0
			b -= MathHelper.TwoPi;
			result = MathHelper.Lerp(a, b, factor);

			if (result < 0f) {
				result += MathHelper.TwoPi;
			}
		} else {
			// Straight lerp
			result = MathHelper.Lerp(a, b, factor);
		}

		return result;
	}

	// Damping

	/// <summary>
	/// Framerate independent damping.<br/>
	/// Based on https://www.rorydriscoll.com/2016/03/07/frame-rate-independent-damping-using-lerp
	/// </summary>
	public static float Damp(float source, float destination, float smoothing, float dt)
	{
		return MathHelper.Lerp(source, destination, 1f - MathF.Pow(smoothing, dt));
	}
	/// <inheritdoc cref="Damp"/>
	public static Vector2 Damp(Vector2 source, Vector2 destination, float smoothing, float dt)
	{
		return new
		(
			Damp(source.X, destination.X, smoothing, dt),
			Damp(source.Y, destination.Y, smoothing, dt)
		);
	}

	// Etc.

	/// <summary> Rounds <paramref name="value"/> to the nearest percent. </summary>
	public static int Percent(float value)
	{
		return (int)Math.Round(value * 100);
	}

	/// <summary> Calculates a [0..1] distance falloff factor given a distance value and start and end range. </summary>
	public static float DistancePower(float distance, float closest, float farthest)
	{
		Debug.Assert(closest >= 0f);
		Debug.Assert(farthest >= 0f);
		Debug.Assert(distance >= 0f);
		Debug.Assert(farthest >= closest);

		float result = 1f - (distance - closest) / (farthest - closest);

		return float.IsNaN(result) ? 0f : Clamp01(result);
	}

	public static bool InsideRange((float Min, float Max) range, float value)
	{
		return value > range.Min && value < range.Max;
	}
	public static bool InsideRangeSqr((float Min, float Max) range, float sqrValue)
	{
		return InsideRange((range.Min * range.Min, range.Max * range.Max), sqrValue);
	}
	public static bool IntersectsRange((float Min, float Max) range, float value)
	{
		return value >= range.Min && value <= range.Max;
	}
	public static bool IntersectsRangeSqr((float Min, float Max) range, float sqrValue)
	{
		return IntersectsRange((range.Min * range.Min, range.Max * range.Max), sqrValue);
	}
}