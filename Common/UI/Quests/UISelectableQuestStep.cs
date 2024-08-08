using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.UI.Utilities;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Quests;

// ReSharper disable once InconsistentNaming
public class UISelectableQuestStep : UISelectableOutlineRectPanel
{
	private UISimpleWrappableText Title { get; set; }
	private QuestStep Step => _quest.QuestSteps[_stepIndex];

	private readonly int _stepIndex = 0;
	private readonly Quest _quest = null;

	public UISelectableQuestStep(int stepIndex, Quest quest)
	{
		_stepIndex = stepIndex;
		_quest = quest;

		DrawFilled = true;
		SelectedFillColour = new Color(102, 86, 67) * QuestsUIState.SelectedOpacity;
		HoverFillColour = new Color(102, 86, 67) * QuestsUIState.HoveredOpacity;
		Height.Set(22f, 0f);
		Width.Set(325, 0f);

		// text
		Title = new UISimpleWrappableText(Step.QuestString(), 0.7f);
		Title.Left.Set(14f, 0f);
		Title.Top.Set(-8f, 0f);
		Title.Colour = new Color(43, 28, 17);

		Title.OnUpdate += UpdateText;

		Append(Title);
	}

	private void UpdateText(UIElement affectedElement)
	{
		var text = affectedElement as UISimpleWrappableText;
		text.SetText(Step.IsDone ? Step.QuestCompleteString() : Step.QuestString());
		text.Colour = Step.IsDone ? new Color(50, 120, 10) : new Color(43, 28, 17);
	}

	public override void LeftMouseDown(UIMouseEvent evt)
	{
		base.LeftMouseDown(evt);
		Main.NewText("Left Clicked on show details");
	}
	
	public override void RightMouseDown(UIMouseEvent evt)
	{
		base.RightMouseDown(evt);
		Main.NewText("Right Clicked on show details");
	}
}