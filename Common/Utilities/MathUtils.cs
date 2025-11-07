using Terraria.UI;

namespace PathOfTerraria.Common.Utilities;

public static class MathUtils
{
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

	/// <summary> Rounds <paramref name="value"/> to the nearest percent. </summary>
	public static int Percent(float value)
	{
		return (int)Math.Round(value * 100);
	}
}