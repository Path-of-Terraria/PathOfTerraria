using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Quests;

internal class QuestDetailsPanel : SmartUiElement
{
	public static QuestModPlayer QuestPlayer => Main.LocalPlayer.GetModPlayer<QuestModPlayer>();

	public string ViewedQuestName { get; set; }

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
		
		if (QuestPlayer.GetQuestCount() != 0 && !string.IsNullOrEmpty(ViewedQuestName))
		{
			string name = Quest.GetQuest(ViewedQuestName).DisplayName.Value;
			Utils.DrawBorderStringBig(spriteBatch, name, GetRectangle().Center() + new Vector2(175, -320), Color.White, 0.5f, 0.5f, 0.35f);
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
		if (string.IsNullOrEmpty(ViewedQuestName) || ViewedQuestName is null)
		{
			return;
		}

		var quest = Quest.GetQuest(ViewedQuestName);
		int index = 0;
		int offset = 0;

		for (int i = 0; i < quest.QuestSteps.Count; i++)
		{
			QuestStep step = quest.QuestSteps[i];

			if (step.NoUI)
			{
				continue;
			}

			var stepUI = new UISelectableQuestStep(i, ViewedQuestName);
			stepUI.Left.Set(530, 0f);
			stepUI.Top.Set(100 + offset * 22, 0f);
			Append(stepUI);
			offset += step.LineCount;
			index++;
		}
	}
}