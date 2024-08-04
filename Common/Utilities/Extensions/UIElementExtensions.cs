using Terraria.UI;

namespace PathOfTerraria.Common.Utilities.Extensions;

/// <summary>
///		Basic <see cref="UIElement"/> extensions.
/// </summary>
public static class UIElementExtensions
{
	/// <summary>
	///		Sets the margin for all sides of the specified element.
	/// </summary>
	/// <remarks>
	///		This will set the margin for all four sides of the element:
	///		<see cref="UIElement.MarginTop"/>, <see cref="UIElement.MarginLeft"/>,
	///		<see cref="UIElement.MarginBottom"/> and <see cref="UIElement.MarginRight"/>
	/// </remarks>
	/// <param name="element">The element to set the margin.</param>
	/// <param name="margin">The margin in pixel units.</param>
	public static void SetMargin(this UIElement element, float margin)
	{
		element.MarginBottom = margin;
		element.MarginLeft = margin;
		element.MarginRight = margin;
		element.MarginTop = margin;
	}
}