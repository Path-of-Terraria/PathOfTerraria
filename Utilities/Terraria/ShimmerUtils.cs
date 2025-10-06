namespace PathOfTerraria.Utilities.Terraria;

internal static class ShimmerUtils
{
	public static Color ShimmerizeColor(Item item, Color baseColor)
	{
		if (!item.shimmered)
		{
			return baseColor;
		}

		baseColor.R = (byte)(baseColor.R * (1f - item.shimmerTime));
		baseColor.G = (byte)(baseColor.G * (1f - item.shimmerTime));
		baseColor.B = (byte)(baseColor.B * (1f - item.shimmerTime));
		baseColor.A = (byte)(baseColor.A * (1f - item.shimmerTime));
		return baseColor;
	}

	public static Color GetShimmeredAlpha(Item item, Color baseColor)
	{
		return ShimmerizeColor(item, item.GetAlpha(baseColor));
	}
}
