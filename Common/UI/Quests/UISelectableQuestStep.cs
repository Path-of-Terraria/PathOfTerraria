using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.UI.Utilities;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Quests;

// ReSharper disable once InconsistentNaming

/// <summary>
/// Displays a single <see cref="QuestStep"/> for a given quest.<br/>
/// It'll automatically update as long as it's active, change color to green when done, gray out when locked, and default otherwise.
/// </summary>
public class UISelectableQuestStep : UISelectableOutlineRectPanel
{
	private UISimpleWrappableText Title { get; set; }
	private Quest Quest => Quest.GetQuest(questName);
	private QuestStep Step => Quest.QuestSteps[index];
	
	private readonly string questName;
	private readonly int index;

	public UISelectableQuestStep(int stepIndex, string quest)
	{
		questName = quest;
		index = stepIndex;

		DrawFilled = true;
		SelectedFillColour = new Color(102, 86, 67) * QuestsUIState.SelectedOpacity;
		HoverFillColour = new Color(102, 86, 67) * QuestsUIState.HoveredOpacity;
		Height.Set(22f, 0f);
		Width.Set(325, 0f);

		// text
		Title = new UISimpleWrappableText(string.Empty, 0.7f);
		Title.Left.Set(14f, 0f);
		Title.Top.Set(-8f, 0f);
		Title.Colour = new Color(43, 28, 17);

		Title.OnUpdate += UpdateText;

		Append(Title);
	}

	private void UpdateText(UIElement affectedElement)
	{
		var text = affectedElement as UISimpleWrappableText;

		if (Step.IsDone && text.Colour.R == 50) // Stop if the step is done
		{
			return;
		}

		text.SetText(Step.DisplayString()); // Update text, and set color 
		text.Colour = Step.IsDone ? new Color(50, 120, 10) : new Color(43, 28, 17);

		if (Quest.CurrentStep < index) // Gray out steps that haven't been approached yet
		{
			text.Colour = new Color(43, 28, 17) * 0.25f;
		}
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