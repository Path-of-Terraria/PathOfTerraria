using PathOfTerraria.Core.Loaders.UILoading;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.SkillsTree;

internal class SkillsTreeInnerPanel : SmartUIElement
{
	private UIElement Panel => Parent;
	
	public bool Visible = false;
	public override string TabName => "SkillTree";

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (!Visible)
		{
			return;
		}

		Utils.DrawBorderStringBig(
			spriteBatch, 
			"Skills - Placeholder",
			GetRectangle().TopLeft() + new Vector2(138, 150),
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