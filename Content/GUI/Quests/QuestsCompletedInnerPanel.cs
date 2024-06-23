using PathOfTerraria.Core.Loaders.UILoading;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.Quests;

internal class QuestsCompletedInnerPanel : SmartUIElement
{
	private UIElement Panel => Parent;
	
	public bool Visible = false;

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (!Visible)
		{
			return;
		}

		Utils.DrawBorderStringBig(
			spriteBatch, 
			"Quests Completed - Placeholder",
			GetRectangle().TopLeft() + new Vector2(180, 150),
			Color.White,
			0.6f,
			0.5f,
			0.35f);
	}
	
	public Rectangle GetRectangle()
	{
		return Panel.GetDimensions().ToRectangle();
	}
}