using Terraria.UI;

namespace PathOfTerraria.Common.Utilities;

public static class MathUtils
{
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
}