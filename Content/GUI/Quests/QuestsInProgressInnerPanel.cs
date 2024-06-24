using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Systems.Questing;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.Quests;

internal class QuestsInProgressInnerPanel : SmartUIElement
{
	private UIElement Panel => Parent;
	private List<Quest> _quests = [];
	public override string TabName => "QuestsAvailable";
	
	public bool Visible = true;

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (!Visible)
		{
			return;
		}

		_quests = Main.LocalPlayer.GetModPlayer<QuestModPlayer>().GetIncompleteQuests();	
		foreach (Quest quest in _quests.Where(x => !x.Completed))
		{
			Utils.DrawBorderStringBig(
				spriteBatch, 
				quest.Name,
				GetRectangle().TopLeft() + new Vector2(25, 150),
				Color.White,
				0.5f,
				0.5f,
				0.35f);
		}
	}

	private Rectangle GetRectangle()
	{
		return Panel.GetDimensions().ToRectangle();
	}
}