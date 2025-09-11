using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Core.UI;

internal static class UIUtilities
{
	/// <summary> Adds or Appends a child element to the current one and returns it. </summary>
	public static T AddElement<T>(this UIElement parent, T child, Action<T> initAction) where T : UIElement
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
}
