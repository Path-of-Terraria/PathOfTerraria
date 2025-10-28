using PathOfTerraria.Core.UI;
using Terraria.UI;

#nullable enable

namespace PathOfTerraria.Common.UI.Components;

internal class UIBlockMouse() : UIComponent
{
	protected override void OnAttach(UIElement element)
	{
		element.OnUpdate += BlockMouse;
	}

	private void BlockMouse(UIElement affectedElement)
	{
		if (affectedElement.ContainsPoint(Main.MouseScreen))
		{
			Main.LocalPlayer.mouseInterface = true;
		}
	}

	protected override void OnDetach(UIElement element)
	{
		element.OnUpdate -= BlockMouse;
	}
}
