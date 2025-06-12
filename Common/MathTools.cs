namespace PathOfTerraria.Common;

internal static class MathTools
{
	internal static float ModDistance(this Vector2 position, Vector2 circleCenter, float xMod, float yMod)
	{
		return MathF.Sqrt(MathF.Pow(position.X - circleCenter.X, 2) * xMod + MathF.Pow(position.Y - circleCenter.Y, 2) * yMod);
	}

	internal static Color GetRainbowColor(float progress, float brightness = 0.5f)
	{
		return Main.hslToRgb(new(progress, brightness, brightness));
	}
}
