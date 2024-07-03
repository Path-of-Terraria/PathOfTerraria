using PathOfTerraria.Content.GUI.Utilities;
using PathOfTerraria.Core.Systems.Questing;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.Quests;

// ReSharper disable once InconsistentNaming
public class UISelectableQuestStep : UISelectableOutlineRectPanel
{
	private QuestStep QuestStep { get; set; }
	private UISimpleWrappableText Title { get; set; }

	public UISelectableQuestStep(QuestStep step)
	{
		DrawFilled = true;
		SelectedFillColour = new Color(102, 86, 67) * QuestsUIState.SELECTED_OPACITY;
		HoverFillColour = new Color(102, 86, 67) * QuestsUIState.HOVERED_OPACITY;
		Height.Set(22f, 0f);
		Width.Set(375, 0f);

		// text
		Title = new UISimpleWrappableText(step.QuestString(), 0.7f);
		Title.Left.Set(14f, 0f);
		Title.Top.Set(-8f, 0f);
		Title.Colour = new Color(43, 28, 17);
		Append(Title);

		QuestStep = step;
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