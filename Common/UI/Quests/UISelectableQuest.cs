using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.UI.Utilities;

namespace PathOfTerraria.Common.UI.Quests;

// ReSharper disable once InconsistentNaming
public class UISelectableQuest : UISelectableOutlineRectPanel
{
	private UISimpleWrappableText Title { get; set; }

	public UISelectableQuest(string quest)
	{
		DrawFilled = true;
		SelectedFillColour = new Color(102, 86, 67) * QuestsUIState.SelectedOpacity;
		HoverFillColour = new Color(102, 86, 67) * QuestsUIState.HoveredOpacity;
		Height.Set(22f, 0f);
		Width.Set(325, 0f);

		// text
		Title = new UISimpleWrappableText(Quest.GetQuest(quest).DisplayName.Value, 0.7f);
		Title.Left.Set(14f, 0f);
		Title.Top.Set(-8f, 0f);
		Title.Colour = new Color(43, 28, 17);
		Append(Title);
	}
}