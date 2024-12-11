using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.UI.Utilities;

namespace PathOfTerraria.Common.UI.Quests;

// ReSharper disable once InconsistentNaming
public class UISelectableQuest : UISelectableOutlineRectPanel
{
	private readonly QuestDetailsPanel Panel = null;
	private readonly string QuestName = null;

	private UISimpleWrappableText Title { get; set; }

	public UISelectableQuest(Quest quest, QuestDetailsPanel panel)
	{
		Panel = panel;
		QuestName = quest.Name;

		DrawFilled = true;
		DrawBorder = true;

		BorderThickness = 2;
		NormalOutlineColour = Color.Transparent;
		SelectedFillColour = new Color(102, 86, 67) * QuestsUIState.SelectedOpacity;
		HoverFillColour = new Color(102, 86, 67) * QuestsUIState.HoveredOpacity;
		Height.Set(22f, 0f);
		Width.Set(325, 0f);

		// text
		Title = new UISimpleWrappableText(quest.DisplayName.Value, 0.7f);
		Title.Left.Set(14f, 0f);
		Title.Top.Set(-8f, 0f);
		Title.Colour = new Color(43, 28, 17);
		Append(Title);
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		Color color = Panel.ViewedQuestName == QuestName ? new Color(102, 86, 67) * QuestsUIState.SelectedOpacity : Color.Transparent;
		HoverOutlineColour = SelectedOutlineColour = NormalOutlineColour = color;
	}
}