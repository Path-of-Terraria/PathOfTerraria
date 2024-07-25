namespace PathOfTerraria.Common.World.Generation.Town;

public class Building(Point size, Building.PlaceDelegate placement, Building.CanPlaceDelegate canPlace = null, Building.GetBoundingBoxDelegate box = null)
{
	public delegate void PlaceDelegate(Point size, Point position);
	public delegate bool CanPlaceDelegate(Point size, Point position);
	public delegate Rectangle GetBoundingBoxDelegate(Point size, Point position);

	public Point Size { get; protected set; } = size;
	
	protected PlaceDelegate Placement = placement;
	protected CanPlaceDelegate CanPlace = canPlace ?? DefaultCanPlace;
	protected GetBoundingBoxDelegate GetBox = box ?? DefaultBox;

	public static Rectangle DefaultBox(Point size, Point position)
	{
		return new Rectangle(position.X - 5, position.Y - 5, size.X + 10, size.Y + 10);
	}

	public static bool DefaultCanPlace(Point size, Point pos)
	{
		int count = CountTileTypesInArea(pos.X, pos.X + size.X, pos.Y + size.Y / 2, pos.Y + size.Y);
		int topCount = CountTileTypesInArea(pos.X, pos.X + size.X, pos.Y, pos.Y + size.Y / 2);
		return count > size.X && count < size.X * 4f && topCount < size.X;
	}

	public bool PlaceAt(Point position)
	{
		if (CanPlace(Size, position))
		{
			Placement(Size, position);
			return true;
		}

		return false;
	}

	public Rectangle GetBoundingBox(Point position)
	{
		return GetBox(Size, position);
	}

	public static int CountTileTypesInArea(int startX, int endX, int startY, int endY)
	{
		int count = 0;

		for (int i = startX; i <= endX; i++)
		{
			for (int j = startY; j <= endY; j++)
			{
				if (WorldGen.SolidTile(i, j))
				{
					count++;
				}
			}
		}

		return count;
	}
}