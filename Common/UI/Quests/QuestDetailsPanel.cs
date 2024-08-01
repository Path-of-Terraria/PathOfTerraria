using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Core.UI;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Quests;

internal class QuestDetailsPanel : SmartUIElement
{
	public Quest ViewedQuest { get; set; }

	public override string TabName => "QuestBookMenu";

	public override void SafeMouseOver(UIMouseEvent evt)
	{
		Main.blockMouse = true;
		Main.isMouseLeftConsumedByUI = true;
		Main.LocalPlayer.mouseInterface = true;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		DrawBack(spriteBatch);
		if (Main.LocalPlayer.GetModPlayer<QuestModPlayer>().GetQuestCount() != 0 && ViewedQuest is not null)
		{
			Utils.DrawBorderStringBig(spriteBatch, ViewedQuest.Name, GetRectangle().Center() + new Vector2(175, -320), Color.White, 0.5f, 0.5f, 0.35f);
		}
#if DEBUG
		GUIDebuggingTools.DrawGuiBorder(spriteBatch, this, Color.Red);
#endif
		base.Draw(spriteBatch);
	}

	private void DrawBack(SpriteBatch spriteBatch)
	{
		Texture2D tex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/QuestBookBackground").Value;
		spriteBatch.Draw(tex, GetRectangle().Center(), null, Color.White, 0f, tex.Size() / 2f, 1f, SpriteEffects.None, 0);
	}

	public void PopulateQuestSteps()
	{
		if (ViewedQuest is null)
		{
			return;
		}

		int index = 0;

		foreach (QuestStep step in ViewedQuest.GetSteps())
		{
			if (index > ViewedQuest.CurrentQuest)
			{
				continue;
			}

			var stepUI = new UISelectableQuestStep(step);
			stepUI.Left.Set(530, 0f);
			stepUI.Top.Set(100 + index * 22, 0f);
			Append(stepUI);
			index++;
		}
	}
}