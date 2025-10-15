namespace PathOfTerraria.Utilities.Xna;

internal readonly struct Vector2Drag(Vector2 startPoint, Vector2 startCursor, Vector2 axis)
{
	public readonly Vector2 StartPoint = startPoint;
	public readonly Vector2 StartCursor = startCursor;
	public readonly Vector2 Axis = axis;

	public readonly Vector2 Calculate(Vector2 dragPoint)
	{
		Vector2 dragDelta = dragPoint - StartCursor;
		Vector2 moveOffset = dragDelta * Axis;
		Vector2 result = StartPoint + moveOffset;

		return result;
	}
}

internal readonly struct RectangleDrag
{
	public readonly Vector2Drag Move;
	public readonly Vector2Drag Resize;

	public RectangleDrag(Vector2 position, Vector2 size, Vector2 cursor, Vector2 moveAxis, Vector2 resizeSigns)
	{
		if (resizeSigns != default)
		{
			Move = new Vector2Drag(position, cursor, Vector2.Max(-resizeSigns, Vector2.Zero));
			Resize = new Vector2Drag(size, cursor, resizeSigns);
		}
		else if (moveAxis != default)
		{
			Move = new Vector2Drag(position, cursor, moveAxis);
			Resize = new Vector2Drag(size, cursor, Vector2.Zero);
		}
		else
		{
			Move = new Vector2Drag(position, cursor, Vector2.Zero);
			Resize = new Vector2Drag(size, cursor, Vector2.Zero);
		}
	}

	public readonly (Vector2 Position, Vector2 Size) Calculate(Vector2 dragPoint)
	{
		Vector2 pos = Move.Calculate(dragPoint);
		Vector2 size = Resize.Calculate(dragPoint);

		return (pos, size);
	}
}
