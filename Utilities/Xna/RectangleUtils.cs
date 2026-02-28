namespace PathOfTerraria.Utilities.Xna;

internal static class RectangleUtils
{
	public static Rectangle Inflated(this Rectangle self, int x, int y)
	{
		self.Inflate(x, y);
		return self;
	}
	public static Rectangle WithOffset(this Rectangle self, int x, int y)
	{
		return self with { X = self.X + x, Y = self.Y + y };
	}
}
