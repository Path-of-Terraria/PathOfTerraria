namespace PathOfTerraria.Common.Utilities;

public abstract class Easings
{
	public static readonly Easings EaseQuadIn = new PolynomialEase((x) => x * x);
	public static readonly Easings EaseQuadOut = new PolynomialEase((x) => 1f - EaseQuadIn.Ease(1f - x));
	public static readonly Easings EaseQuadInOut = new PolynomialEase((x) => x < 0.5f ? 2f * x * x : -2f * x * x + 4f * x - 1f);

	public static readonly Easings EaseCubicIn = new PolynomialEase((x) => x * x * x);
	public static readonly Easings EaseCubicOut = new PolynomialEase((x) => 1f - EaseCubicIn.Ease(1f - x));
	public static readonly Easings EaseCubicInOut = new PolynomialEase((x) => x < 0.5f ? 4f * x * x * x : 4f * x * x * x - 12f * x * x + 12f * x - 3f);

	public static readonly Easings EaseQuarticIn = new PolynomialEase((x) => x * x * x * x);
	public static readonly Easings EaseQuarticOut = new PolynomialEase((x) => 1f - EaseQuarticIn.Ease(1f - x));
	public static readonly Easings EaseQuarticInOut = new PolynomialEase(
		(x) => x < 0.5f ? 8f * x * x * x * x : -8f * x * x * x * x + 32f * x * x * x - 48f * x * x + 32f * x - 7f);

	public static readonly Easings EaseQuinticIn = new PolynomialEase((x) => x * x * x * x * x);
	public static readonly Easings EaseQuinticOut = new PolynomialEase((x) => 1f - EaseQuinticIn.Ease(1f - x));
	public static readonly Easings EaseQuinticInOut = new PolynomialEase(
		(x) => x < 0.5f ? 16f * x * x * x * x * x : 16f * x * x * x * x * x - 80f * x * x * x * x + 160f * x * x * x - 160f * x * x + 80f * x - 15f);

	public static readonly Easings EaseCircularIn = new PolynomialEase((x) => 1f - (float)Math.Sqrt(1.0 - Math.Pow(x, 2)));
	public static readonly Easings EaseCircularOut = new PolynomialEase((x) => (float)Math.Sqrt(1.0 - Math.Pow(x - 1.0, 2)));
	public static readonly Easings EaseCircularInOut = new PolynomialEase(
		(x) => x < 0.5f ? (1f - (float)Math.Sqrt(1.0 - Math.Pow(x * 2, 2))) * 0.5f : (float)((Math.Sqrt(1.0 - Math.Pow(-2 * x + 2, 2)) + 1) * 0.5));

	public abstract float Ease(float time);
}

public class PolynomialEase(Func<float, float> func) : Easings
{
	private readonly Func<float, float> _function = func;

	public override float Ease(float time)
	{
		return _function(time);
	}
}