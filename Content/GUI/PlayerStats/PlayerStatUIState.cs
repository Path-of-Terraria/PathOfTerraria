using PathOfTerraria.Core.Loaders.UILoading;
using System.Collections.Generic;
using Terraria.Localization;

namespace PathOfTerraria.Content.GUI.PlayerStats;

internal class PlayerStatUIState : DraggableSmartUi
{
	private readonly PlayerStatInnerPanel _mainPanel = new();
	public override List<SmartUIElement> TabPanels => [_mainPanel];

	public override int DepthPriority => 2;

	public void Toggle()
	{
		if (IsVisible)
		{
			IsVisible = false;
			return;
		}

		if (Panel is null)
		{
			RemoveAllChildren();
			var localizedTexts = new (string key, LocalizedText text)[]
			{
				(_mainPanel.TabName, Language.GetText($"Mods.PathOfTerraria.GUI.{_mainPanel.TabName}Tab")),
			};

			base.CreateMainPanel(localizedTexts, panelSize: new Point(506, 496), canResize: false);
			base.AppendChildren();
		}

		IsVisible = true;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Recalculate();
		base.Draw(spriteBatch);
	}
}
