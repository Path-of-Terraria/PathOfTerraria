using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Systems.Questing;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.Quests;

internal class QuestsCompletedInnerPanel() : SmartUIElement
{
	private UIElement Panel => Parent;
	public Quest ViewingQuest;

	public override string TabName => "QuestBookMenu";

	public override void Draw(SpriteBatch spriteBatch)
	{
		DrawBack(spriteBatch);
		if (ViewingQuest is not null)
		{
			Utils.DrawBorderStringBig(
				spriteBatch, 
				ViewingQuest.Name,
				GetRectangle().Center() + new Vector2(-220, -385),
				Color.White, 
				0.5f, 
				0.5f, 
				0.35f);
		}
		else
		{
			Utils.DrawBorderStringBig(
				spriteBatch,
				"NO QUESTS",
				GetRectangle().Center() + new Vector2(0, -20),
				Color.White, 
				0.5f, 
				0.5f, 
				0.35f);
		}
	}

	private void DrawBack(SpriteBatch spriteBatch)
	{
		Texture2D tex = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/QuestBookBackground").Value;
		spriteBatch.Draw(tex, GetRectangle().Center(), null, Color.White, 0f, tex.Size() / 2f, 1f, SpriteEffects.None, 0);
	}
	
	private Rectangle GetRectangle()
	{
		return Panel.GetDimensions().ToRectangle();
	}
}