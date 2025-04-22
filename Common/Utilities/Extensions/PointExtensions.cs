using Terraria.DataStructures;

namespace PathOfTerraria.Common.Utilities.Extensions;

public static class PointExtensions
{
	public static Point16 ToPoint16(this Point point)
	{
		return new Point16(point.X, point.Y);
	}
}