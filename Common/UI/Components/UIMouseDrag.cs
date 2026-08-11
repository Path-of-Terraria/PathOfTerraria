using PathOfTerraria.Core.UI;
using PathOfTerraria.Utilities.Xna;
using Terraria.UI;

#nullable enable

namespace PathOfTerraria.Common.UI.Components;

/// <summary>
/// Makes an element movable and resizable using the left mouse button, preserving its last dimensions by identifier.
/// </summary>
internal sealed class UIMouseDrag(string persistenceIdentifier, bool canMove, bool canResize) : UIComponent
{
	private const float ViewportPadding = 8f;

	private RectangleDrag drag;
	private readonly UIPersistent persistence = new(persistenceIdentifier, preservePosition: canMove || canResize, preserveSize: canResize);

	public bool CanMove { get; set;  } = canMove;
	public bool CanResize { get; set; } = canResize;
	public string PersistenceIdentifier => persistence.Identifier;

	public bool IsDragging => drag.Move.Axis != default || drag.Resize.Axis != default;

	protected override void OnAttach(UIElement element)
	{
		element.OnUpdate += OnUpdate;
		element.OnLeftMouseDown += OnLeftMouseDown;
		persistence.AttachTo(element);
	}
	protected override void OnDetach(UIElement element)
	{
		persistence.DetachFrom(element);
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

		if (IsDragging)
		{
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
		}

		KeepInsideViewport(element);

		if (IsDragging && !Main.mouseLeft)
		{
			drag = default;
		}
	}

	private static void KeepInsideViewport(UIElement element)
	{
		if (element.Parent is not UIElement parent)
		{
			return;
		}

		CalculatedStyle viewport = parent.GetInnerDimensions();
		if (viewport.Width <= 0f || viewport.Height <= 0f)
		{
			return;
		}

		CalculatedStyle bounds = element.GetOuterDimensions();
		float availableWidth = Math.Max(0f, viewport.Width - ViewportPadding * 2f);
		float availableHeight = Math.Max(0f, viewport.Height - ViewportPadding * 2f);

		float targetX = bounds.Width <= availableWidth
			? Math.Clamp(bounds.X, viewport.X + ViewportPadding, viewport.X + viewport.Width - ViewportPadding - bounds.Width)
			: viewport.X + (viewport.Width - bounds.Width) * 0.5f;
		float targetY = bounds.Height <= availableHeight
			? Math.Clamp(bounds.Y, viewport.Y + ViewportPadding, viewport.Y + viewport.Height - ViewportPadding - bounds.Height)
			: viewport.Y + (viewport.Height - bounds.Height) * 0.5f;

		Vector2 correction = new(targetX - bounds.X, targetY - bounds.Y);
		if (Math.Abs(correction.X) <= 0.01f && Math.Abs(correction.Y) <= 0.01f)
		{
			return;
		}

		element.Left.Pixels += correction.X;
		element.Top.Pixels += correction.Y;
		element.Recalculate();
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
