using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Core.UI;

internal static class UIUtilities
{
	/// <summary> Adds or Appends a child element to the current one and returns it. </summary>
	public static T AddElement<T>(this UIElement parent, T child, Action<T> initAction = null) where T : UIElement
	{
		initAction?.Invoke(child);

		if (parent is UIGrid uiGrid)
		{
			uiGrid.Add(child);
		}
		else if (parent is UIList uiList)
		{
			uiList.Add(child);
		}
		else
		{
			parent.Append(child);
		}

		return child;
	}

	/// <summary> Sets an element's location and size. </summary>
	public static void SetDimensions
	(
		this UIElement element,
		(float Factor, float Pixels) x = default,
		(float Factor, float Pixels) y = default,
		(float Factor, float Pixels) width = default,
		(float Factor, float Pixels) height = default
	)
	{
		element.Left = new StyleDimension(x.Pixels, x.Factor);
		element.Top = new StyleDimension(y.Pixels, y.Factor);
		element.Width = new StyleDimension(width.Pixels, width.Factor);
		element.Height = new StyleDimension(height.Pixels, height.Factor);
	}

	/// <summary> Copies a provided element's dimensions to this one. </summary>
	public static void CopyDimensionsFrom(this UIElement target, UIElement source)
	{
		target.Left = source.Left;
		target.Top = source.Top;
		target.Width = source.Width;
		target.Height = source.Height;
	}
}
