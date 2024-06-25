using System.Collections.Generic;
using PathOfTerraria.Core.Loaders.UILoading;
using Terraria.Localization;

namespace PathOfTerraria.Content.GUI.Quests;

internal class QuestsUIState : DraggableSmartUi
{
	private readonly QuestsCompletedInnerPanel _questsCompletedInnerPanel = new();
	private readonly QuestsInProgressInnerPanel _questsInProgressInnerPanel = new();
	public override List<SmartUIElement> TabPanels => [_questsInProgressInnerPanel, _questsCompletedInnerPanel];

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
			(_questsInProgressInnerPanel.TabName, Language.GetText($"Mods.PathOfTerraria.GUI.{_questsInProgressInnerPanel.TabName}Tab")),
			(_questsCompletedInnerPanel.TabName, Language.GetText($"Mods.PathOfTerraria.GUI.{_questsCompletedInnerPanel.TabName}Tab")),
			};

			base.CreateMainPanel(localizedTexts);
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