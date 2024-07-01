using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Systems.Questing;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.Quests;

internal class QuestListPanel : SmartUIElement
{
	private UIElement Panel => Parent;
	public Quest ViewedQuest { get; set; }
	private int currentQuestIndex = 0;

	public QuestListPanel()
	{
		Main.LocalPlayer.GetModPlayer<QuestModPlayer>().GetQuests();
	}

	public override string TabName => "QuestBookMenu";

	public override void Draw(SpriteBatch spriteBatch)
	{
		DrawBack(spriteBatch);
		if (Main.LocalPlayer.GetModPlayer<QuestModPlayer>().GetQuestCount() != 0)
		{
			string name = Main.LocalPlayer.GetModPlayer<QuestModPlayer>().GetQuests()[currentQuestIndex];
			string text = Main.LocalPlayer.GetModPlayer<QuestModPlayer>().GetQuestSteps(name);
			Utils.DrawBorderStringBig(spriteBatch, text, GetRectangle().Center() + new Vector2(-220, -265), Color.White, 0.5f, 0.5f, 0.35f);
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
}