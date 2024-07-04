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
			Utils.DrawBorderStringBig(spriteBatch, ViewedQuest.Name, GetRectangle().Center() + GetQuestNamePosition(), Color.White, 0.5f, 0.5f, 0.35f);
		}

		base.Draw(spriteBatch);
	}

	private void DrawBack(SpriteBatch spriteBatch)
	{
		Texture2D tex = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/QuestBookBackground").Value;
		spriteBatch.Draw(tex, GetRectangle().Center() + GetBackPosition(), null, Color.White, 0f, tex.Size() / 2f, 1f, SpriteEffects.None, 0);
	}
	
	private static Vector2 GetQuestNamePosition()
	{
		float screenWidth = Main.screenWidth / 1.12f;
		return screenWidth switch
		{
			//4k or 4k Wide
			>= 2160 => new Vector2(325, -130),
			//1440p+
			>= 1440 => new Vector2(225, -225),
			_ => new Vector2(175, -320)
		};
	}
	
	private static Vector2 GetBackPosition()
	{
		float screenWidth = Main.screenWidth / 1.12f;
		return screenWidth switch
		{
			//4k or 4k Wide
			>= 2160 => new Vector2(200, 200),
			//1440p+
			>= 1440 => new Vector2(100, 100),
			_ => new Vector2(0, 0)
		};
	}
	
	private static float GetQuestStepLeft()
	{
		float screenWidth = Main.screenWidth / 1.12f;
		return screenWidth switch
		{
			//4k or 4k Wide
			>= 2160 => 740,
			//1440p+
			>= 1440 => 640,
			_ => 550
		};
	}
	
	private static float GetQuestStepTop()
	{
		float screenWidth = Main.screenWidth / 1.12f;
		return screenWidth switch
		{
			//4k or 4k Wide
			>= 2160 => 325,
			//1440p+
			>= 1440 => 240,
			_ => 125
		};
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
			stepUI.Top.Set(GetQuestStepTop() + offset, 0f);
			stepUI.Left.Set(GetQuestStepLeft(), 0f);
			Append(stepUI);
			offset += 22;
			index++;
		}
	}
}