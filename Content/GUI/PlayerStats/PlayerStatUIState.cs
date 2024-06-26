using PathOfTerraria.Core.Loaders.UILoading;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.PlayerStats;

internal class PlayerStatUIState : DraggableSmartUi
{
	private readonly PlayerStatInnerPanel _mainPanel = new() { Width = StyleDimension.FromPixels(512), Height = StyleDimension.FromPixels(448), HAlign = 0.5f, VAlign = 0.55f };
	public override List<SmartUIElement> TabPanels => [_mainPanel];

	public override int DepthPriority => 2;

	public void Toggle()
	{
		if (IsVisible)
		{
			IsVisible = false;
			return;
		}

		if (!HasChild(_mainPanel))
		{
			RemoveAllChildren();
			Append(_mainPanel);
		}

		IsVisible = true;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Recalculate();
		base.Draw(spriteBatch);
	}
}
