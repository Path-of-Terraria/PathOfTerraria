using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Systems.Questing;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.Quests;

internal class QuestsCompletedInnerPanel : SmartUIElement
{
	private UIElement Panel => Parent;
	private List<Quest> _quests = [];
	public override string TabName => "QuestsCompleted";
	
	public bool Visible = false;

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (!Visible)
		{
			return;
		}

		_quests = Main.LocalPlayer.GetModPlayer<QuestModPlayer>().GetCompletedQuests();	
		foreach (Quest quest in _quests.Where(x => x.Completed))
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
	
	public Rectangle GetRectangle()
	{
		return Panel.GetDimensions().ToRectangle();
	}
}