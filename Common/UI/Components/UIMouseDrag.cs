using PathOfTerraria.Core.UI;
using PathOfTerraria.Utilities.Xna;
using Terraria.UI;

#nullable enable

namespace PathOfTerraria.Common.UI.Components;

/// <summary> Makes an element movable and resizable using the left mouse button. </summary>
internal sealed class UIMouseDrag(bool canMove, bool canResize) : UIComponent
{
	private RectangleDrag drag;

	public bool CanMove { get; set;  } = canMove;
	public bool CanResize { get; set; } = canResize;

	public bool IsDragging => drag.Move.Axis != default || drag.Resize.Axis != default;

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

		if (IsDragging ? (drag.Resize.Axis != Vector2.Zero) : (CanResize && CheckResizeArea(element, mousePosition, out _)))
		{
			Main.cursorOverride = 2;
		}

		if (!IsDragging)
		{
			return;
		}

		//TODO: Deal in calculated space, to account for factors and not violate min/max sizes.
		Vector2 oldPosition = new(element.Left.Pixels, element.Top.Pixels);
		Vector2 oldSize = new(element.Width.Pixels, element.Height.Pixels);
		(Vector2 newPosition, Vector2 newSize) = drag.Calculate(mousePosition);

		if (newPosition != oldPosition || newSize != oldSize)
		{
			element.Left.Pixels = newPosition.X;
			element.Top.Pixels = newPosition.Y;
			element.Width.Pixels = newSize.X;
			element.Height.Pixels = newSize.Y;
			element.Recalculate();
		}

		if (!Main.mouseLeft)
		{
			drag = default;
		}
	}

	private void OnLeftMouseDown(UIMouseEvent evt, UIElement element)
	{
		if (IsDragging || evt.Target != element)
		{
			return;
		}

		Vector2 mousePosition = Main.MouseScreen;
		Vector2 elemPos = new(element.Left.Pixels, element.Top.Pixels);
		Vector2 elemSize = new(element.Width.Pixels, element.Height.Pixels);
		
		CheckResizeArea(element, mousePosition, out Vector2 resizeSigns);

		drag = new RectangleDrag(elemPos, elemSize, mousePosition, CanMove ? Vector2.One : Vector2.Zero, CanResize ? resizeSigns : Vector2.Zero);
	}

	private static bool CheckResizeArea(UIElement element, Vector2 mousePosition, out Vector2 resizeSigns)
	{
		const float ResizeRadius = 12f;

		var dimensions = element.GetOuterDimensions().ToRectangle();

		if (!dimensions.Contains(mousePosition.ToPoint()))
		{
			resizeSigns = default;
			return false; 
		}

		resizeSigns.X = (mousePosition.X >= dimensions.Right - ResizeRadius) ? 1f : ((mousePosition.X <= dimensions.Left + ResizeRadius) ? -1f : 0f);
		resizeSigns.Y = (mousePosition.Y >= dimensions.Bottom - ResizeRadius) ? 1f : ((mousePosition.Y <= dimensions.Top + ResizeRadius) ? -1f : 0f);

		return resizeSigns != default;
	}
}
