using PathOfTerraria.Content.GUI.Utilities;
using Terraria.Localization;

namespace PathOfTerraria.Content.GUI.Quests;

internal class QuestsUIState : DraggableSmartUi
{
	private QuestsCompletedInnerPanel _questsCompletedInnerPanel;
	private QuestsInProgressInnerPanel _questsInProgressInnerPanel;

	public void Toggle()
	{
		if (IsVisible)
		{
			IsVisible = false;
			return;
		}

		RemoveAllChildren();
		CreateMainPanel();
		AddPassiveTreeInnerPanel();
		AddSkillsTreeInnerPanel();
		AddCloseButton();

		IsVisible = true;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Recalculate();
		base.Draw(spriteBatch);
	}

	protected void CreateMainPanel()
	{
		var localizedTexts = new (string key, LocalizedText text)[]
		{
			("QuestAvailableTab", Language.GetText("Mods.PathOfTerraria.GUI.QuestAvailableTab")),
			("QuestCompletedTab", Language.GetText("Mods.PathOfTerraria.GUI.QuestCompletedTab"))
		};
		Panel = new UIDraggablePanel(false, false, localizedTexts, DraggablePanelHeight);
		Panel.OnActiveTabChanged += HandleActiveTabChanged;
		Panel.Left.Set(LeftPadding, 0.5f);
		Panel.Top.Set(TopPadding, 0.5f);
		Panel.Width.Set(PanelWidth, 0);
		Panel.Height.Set(PanelHeight, 0);
		Append(Panel);
	}

	private void HandleActiveTabChanged()
	{
		switch (Panel.ActiveTab)
		{
			case "QuestAvailableTab":
				_questsCompletedInnerPanel.Visible = false;
				_questsInProgressInnerPanel.Visible = true;
				break;
			case "QuestCompletedTab":
				_questsCompletedInnerPanel.Visible = true;
				_questsInProgressInnerPanel.Visible = false;
				break;
		}
	}

	protected void AddPassiveTreeInnerPanel()
	{
		_questsCompletedInnerPanel = new QuestsCompletedInnerPanel();
		_questsCompletedInnerPanel.Left.Set(0, 0);
		_questsCompletedInnerPanel.Top.Set(DraggablePanelHeight, 0);
		_questsCompletedInnerPanel.Width.Set(0, 1f);
		_questsCompletedInnerPanel.Height.Set(-DraggablePanelHeight, 1f);
		Panel.Append(_questsCompletedInnerPanel);
	}
	
	protected void AddSkillsTreeInnerPanel()
	{
		_questsInProgressInnerPanel = new QuestsInProgressInnerPanel();
		_questsInProgressInnerPanel.Left.Set(0, 0);
		_questsInProgressInnerPanel.Top.Set(DraggablePanelHeight, 0);
		_questsInProgressInnerPanel.Width.Set(0, 1f);
		_questsInProgressInnerPanel.Height.Set(-DraggablePanelHeight, 1f);
		Panel.Append(_questsInProgressInnerPanel);
	}
}