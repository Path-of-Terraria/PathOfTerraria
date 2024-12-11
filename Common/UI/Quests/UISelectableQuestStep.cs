using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.UI.Utilities;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.UI.Quests;

// ReSharper disable once InconsistentNaming

/// <summary>
/// Displays a single <see cref="QuestStep"/> for a given quest.<br/>
/// It'll automatically update as long as it's active, change color to green when done, gray out when locked, and default otherwise.
/// </summary>
public class UISelectableQuestStep : UISelectableOutlineRectPanel
{
	private UISimpleWrappableText Title { get; set; }
	private QuestStep Step => quest.QuestSteps[index];
	
	private readonly Quest quest;
	private readonly int index;

	private bool _setSize = false;

	public UISelectableQuestStep(int stepIndex, Quest quest)
	{
		this.quest = quest;
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

		Title.OnUpdate += _ => UpdateText();

		Append(Title);
	}

	public void UpdateText()
	{
		if (Step.IsDone && Title.Colour.R == 50 && !_setSize) // Stop if the step is done
		{
			return;
		}

		string textString = Step.DisplayString();
		Title.SetText(textString); // Update text, and set color 
		Title.Colour = Step.IsDone ? new Color(50, 120, 10) : new Color(43, 28, 17);
		Vector2 stringSize = ChatManager.GetStringSize(FontAssets.ItemStack.Value, textString, Vector2.One);
		Title.Height = StyleDimension.FromPixels(stringSize.Y);

		if (quest.CurrentStep < index) // Gray out steps that haven't been approached yet
		{
			Title.Colour = new Color(43, 28, 17) * 0.25f;
		}

		_setSize = true;
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