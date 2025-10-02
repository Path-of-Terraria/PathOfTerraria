namespace PathOfTerraria.Utilities.Xna;

internal static class RectangleUtils
{
	public static Rectangle Inflated(this Rectangle self, int x, int y)
	{
		self.Inflate(x, y);
		return self;
	}
}
