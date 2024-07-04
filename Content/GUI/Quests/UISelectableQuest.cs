using PathOfTerraria.Content.GUI.Utilities;
using PathOfTerraria.Core.Systems.Questing;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.Quests;

// ReSharper disable once InconsistentNaming
public class UISelectableQuest : UISelectableOutlineRectPanel
	{
		private Quest Quest { get; set; }
		private UISimpleWrappableText Title { get; set; }

		private readonly QuestsUIState _state;

		public UISelectableQuest(Quest quest, QuestsUIState state)
		{
			_state = state;

			DrawFilled = true;
			SelectedFillColour = new Color(102, 86, 67) * QuestsUIState.SELECTED_OPACITY;
			HoverFillColour = new Color(102, 86, 67) * QuestsUIState.HOVERED_OPACITY; 
			Height.Set(22f, 0f);
			Width.Set(325, 0f);

			// text
			Title = new UISimpleWrappableText(quest.Name, 0.7f);
			Title.Left.Set(14f, 0f);
			Title.Top.Set(-8f, 0f);
			Title.Colour = new Color(43, 28, 17);
			Append(Title);

			Quest = quest;
		}

		public override void LeftMouseDown(UIMouseEvent evt)
		{
			base.LeftMouseDown(evt);

			_state.SelectQuest(Quest);
		}
	}