using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Core.UI.SmartUI;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Quests;

public class QuestDetailsPanel : SmartUiElement
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
			string name = QuestPlayer.QuestsByName[ViewedQuestName].DisplayName.Value;
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

		UIElement oldList = Children.FirstOrDefault(x => x is UIList);

		if (oldList is not null)
		{
			RemoveChild(oldList);
		}

		Quest quest = Main.LocalPlayer.GetModPlayer<QuestModPlayer>().QuestsByName[ViewedQuestName];
		var list = new UIList()
		{
			Left = StyleDimension.FromPixels(530),
			Top = StyleDimension.FromPixels(100),
			Width = StyleDimension.FromPixels(300),
			Height = StyleDimension.FromPixelsAndPercent(-120, 1)
		};

		for (int i = 0; i < quest.QuestSteps.Count; i++)
		{
			QuestStep step = quest.QuestSteps[i];

			if (step.NoUI)
			{
				continue;
			}

			var stepUI = new UISelectableQuestStep(i, quest)
			{
				Width = StyleDimension.Fill,
				Height = StyleDimension.FromPixels(step.LineCount * 22)
			};

			stepUI.UpdateText();
			list.Add(stepUI);
		}

		Append(list);
	}
}