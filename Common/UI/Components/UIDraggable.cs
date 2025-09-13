using PathOfTerraria.Core.UI;
using Terraria.UI;

#nullable enable

namespace PathOfTerraria.Common.UI.Components;

/// <summary> Makes an element movable and resizable using the left mouse button. </summary>
internal sealed class UIMouseDrag(bool canMove, bool canResize) : UIComponent
{
	private Vector2 dragMousePosition;
	private Vector2 dragElementPosition;
	private Vector2 dragElementSize;
	private Vector2 dragMoveAxes;
	private Vector2 dragResizeAxes;

	public bool CanMove { get; set;  } = canMove;
	public bool CanResize { get; set; } = canResize;
	public bool IsDragging { get; private set; }

	protected override void OnAttach(UIElement element)
	{
		element.OnUpdate += OnUpdate;
		element.OnLeftMouseDown += OnLeftMouseDown;
	}
	protected override void OnDetach(UIElement element)
	{
		element.OnUpdate -= OnUpdate;
		element.OnLeftMouseDown -= OnLeftMouseDown;
	}

	private void OnUpdate(UIElement element)
	{
		Vector2 mousePosition = Main.MouseScreen;

		if (IsDragging ? (dragResizeAxes != Vector2.Zero) : (CanResize && CheckResizeArea(element, mousePosition, out _, out _, out _, out _)))
		{
			Main.cursorOverride = 2;
		}

		if (!IsDragging)
		{
			return;
		}

		Vector2 mouseDelta = mousePosition - dragMousePosition;
		Vector2 oldPosition = new(element.Left.Pixels, element.Top.Pixels);
		Vector2 oldSize = new(element.Width.Pixels, element.Height.Pixels);

		Vector2 moveOffset = mouseDelta * dragMoveAxes;
		Vector2 resizeOffset = mouseDelta * dragResizeAxes;
		element.Left.Pixels = dragElementPosition.X + moveOffset.X;
		element.Top.Pixels = dragElementPosition.Y + moveOffset.Y;
		element.Width.Pixels = dragElementSize.X + resizeOffset.X;
		element.Height.Pixels = dragElementSize.Y + resizeOffset.Y;

		if (element.Left.Pixels != oldPosition.X || element.Top.Pixels != oldPosition.Y || element.Width.Pixels != oldSize.X || element.Height.Pixels != oldSize.Y)
		{
			element.Recalculate();
		}

		if (!Main.mouseLeft)
		{
			IsDragging = false;
		}
	}

	private void OnLeftMouseDown(UIMouseEvent evt, UIElement element)
	{
		if (IsDragging || evt.Target != element)
		{
			return;
		}

		Vector2 mousePosition = Main.MouseScreen;

		if (CanResize)
		{
			var dimensions = element.GetOuterDimensions().ToRectangle();
			bool isResizing = CheckResizeArea(element, mousePosition, out bool left, out bool right, out bool top, out bool bottom);

			if (!CanMove && !isResizing)
			{
				return;
			}

			dragResizeAxes = isResizing ? new Vector2(left ? -1f : (right ? 1f : 0f), top ? -1f : (bottom ? 1f : 0f)) : default;
			dragMoveAxes = isResizing ? new Vector2(left ? 1f : 0f, top ? 1f : 0f) : Vector2.One;
		}
		else
		{
			dragResizeAxes = Vector2.Zero;
			dragMoveAxes = Vector2.One;
		}

		IsDragging = true;
		dragMousePosition = mousePosition;
		dragElementPosition = new Vector2(element.Left.Pixels, element.Top.Pixels);
		dragElementSize = new Vector2(element.Width.Pixels, element.Height.Pixels);
	}

	private static bool CheckResizeArea(UIElement element, Vector2 mousePosition, out bool resizingLeft, out bool resizingRight, out bool resizingTop, out bool resizingBottom)
	{
		const float ResizeRadius = 12f;

		var dimensions = element.GetOuterDimensions().ToRectangle();

		if (!dimensions.Contains(mousePosition.ToPoint()))
		{
			(resizingLeft, resizingRight, resizingTop, resizingBottom) = (false, false, false, false);
			return false; 
		}

		resizingRight = mousePosition.X >= dimensions.Right - ResizeRadius;
		resizingLeft = !resizingRight && mousePosition.X <= dimensions.Left + ResizeRadius;
		resizingBottom = mousePosition.Y >= dimensions.Bottom - ResizeRadius;
		resizingTop = !resizingBottom && mousePosition.Y <= dimensions.Top + ResizeRadius;

		return resizingLeft | resizingRight | resizingTop | resizingBottom;
	}
}
