#pragma warning disable IDE0022 // Use block body for method

namespace PathOfTerraria.Common.Utilities;

public static class Easings
{
	public static float QuadIn(float x) => x * x;
	public static float QuadOut(float x) => 1f - QuadIn(1f - x);
	public static float QuadInOut(float x) => x < 0.5f ? 2f * x * x : -2f * x * x + 4f * x - 1f;

	public static float CubicIn(float x) => x * x * x;
	public static float CubicOut(float x) => 1f - CubicIn(1f - x);
	public static float CubicInOut(float x) => x < 0.5f ? 4f * x * x * x : 4f * x * x * x - 12f * x * x + 12f * x - 3f;

	public static float QuarticIn(float x) => x * x * x * x;
	public static float QuarticOut(float x) => 1f - QuarticIn(1f - x);
	public static float QuarticInOut(float x) => x < 0.5f ? 8f * x * x * x * x : -8f * x * x * x * x + 32f * x * x * x - 48f * x * x + 32f * x - 7f;

	public static float QuinticIn(float x) => x * x * x * x * x;
	public static float QuinticOut(float x) => 1f - QuinticIn(1f - x);
	public static float QuinticInOut(float x) => x < 0.5f ? 16f * x * x * x * x * x : 16f * x * x * x * x * x - 80f * x * x * x * x + 160f * x * x * x - 160f * x * x + 80f * x - 15f;

	public static float CircularIn(float x) => 1f - (float)Math.Sqrt(1.0 - Math.Pow(x, 2));
	public static float CircularOut(float x) => (float)Math.Sqrt(1.0 - Math.Pow(x - 1.0, 2));
	public static float CircularInOut(float x) => x < 0.5f ? (1f - (float)Math.Sqrt(1.0 - Math.Pow(x * 2, 2))) * 0.5f : (float)((Math.Sqrt(1.0 - Math.Pow(-2 * x + 2, 2)) + 1) * 0.5);

	public static float FadeIn(float x, float start, float end) => MathUtils.Clamp01((x - start) / (end - start));
	public static float FadeOut(float x, float start, float end) => MathUtils.Clamp01(1f - (x - start) / (end - start));
}
