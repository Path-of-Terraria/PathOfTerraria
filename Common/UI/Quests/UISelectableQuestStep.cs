using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.UI.Utilities;
using PathOfTerraria.Content.NPCs.BossDomain.Mech;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Quests;

// ReSharper disable once InconsistentNaming

/// <summary>
/// Displays a single <see cref="QuestStep"/> for a given quest.<br/>
/// It'll automatically update as long as it's active, change color to green when done, gray out when locked, and default otherwise.
/// </summary>
public class UISelectableQuestStep : UISelectableOutlineRectPanel
{
	private QuestStep Step => quest.QuestSteps[index];
	
	private readonly Quest quest;
	private readonly int index;

	public UISelectableQuestStep(int stepIndex, Quest quest)
	{
		this.quest = quest;
		index = stepIndex;

		DrawFilled = true;
		SelectedFillColour = new Color(102, 86, 67) * QuestsUIState.SelectedOpacity;
		HoverFillColour = new Color(102, 86, 67) * QuestsUIState.HoveredOpacity;
		Height.Set(22f, 0f);
		Width.Set(325, 0f);
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);

		Vector2 pos = GetDimensions().ToRectangle().TopLeft() + new Vector2(6);
		QuestStep.StepCompletion completion = QuestStep.StepCompletion.Locked;

		if (quest.CurrentStep == index)
		{
			completion = QuestStep.StepCompletion.Current;
		}
		else if (quest.CurrentStep > index)
		{
			completion = QuestStep.StepCompletion.Completed;
		}

		Step.DrawQuestStep(pos, out int height, completion);
		Height = StyleDimension.FromPixels(height);
	}
}