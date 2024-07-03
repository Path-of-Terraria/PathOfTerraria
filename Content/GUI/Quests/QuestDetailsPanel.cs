using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Systems.Questing;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.Quests;

internal class QuestDetailsPanel : SmartUIElement
{
	private UIElement Panel => Parent;
	public Quest ViewedQuest { get; set; }

	public override string TabName => "QuestBookMenu";

	public override void Draw(SpriteBatch spriteBatch)
	{
		DrawBack(spriteBatch);
		if (Main.LocalPlayer.GetModPlayer<QuestModPlayer>().GetQuestCount() != 0)
		{
			Utils.DrawBorderStringBig(spriteBatch, ViewedQuest.Name, GetRectangle().Center() + new Vector2(200, -380), Color.White, 0.5f, 0.5f, 0.35f);
		}

		base.Draw(spriteBatch);
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

	public void PopulateQuestSteps()
	{
		int offset = 0;
		int index = 0;
		foreach (QuestStep step in ViewedQuest.GetSteps())
		{
			if (index > ViewedQuest.CurrentQuest)
			{
				continue;
			}

			var stepUI = new UISelectableQuestStep(step);
			stepUI.Top.Set(150 + offset, 0f);
			stepUI.Left.Set(650, 0f);
			Append(stepUI);
			offset += 22;
			index++;
		}
	}
}