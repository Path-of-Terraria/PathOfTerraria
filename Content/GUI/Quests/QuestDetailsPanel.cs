using PathOfTerraria.Content.GUI.Utilities;
using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Systems.Questing;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.Quests;

internal class QuestDetailsPanel : SmartUIElement
{
	private static readonly Asset<Texture2D> Texture =
		ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/ArrowExtraSmall");

	private UIElement Panel => Parent;
	public Quest ViewedQuest { get; set; }
	private int _currentQuestIndex;

	public QuestDetailsPanel()
	{
		UiFlippableImageButton rightArrow = new(Texture, this);
		UiFlippableImageButton leftArrow = new(Texture, this) { FlipHorizontally = true };

		rightArrow.Left.Set(-230, 1f);
		rightArrow.Top.Set(-180, 1f);
		rightArrow.Width.Set(128, 0);
		rightArrow.Height.Set(128, 0);
		rightArrow.OnLeftClick += (a, b) =>
		{
			if (_currentQuestIndex == Main.LocalPlayer.GetModPlayer<QuestModPlayer>().GetQuestCount() - 1)
			{
				return;
			}

			SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
			_currentQuestIndex++;

			Append(leftArrow);

			if (_currentQuestIndex == Main.LocalPlayer.GetModPlayer<QuestModPlayer>().GetQuestCount() - 1)
			{
				rightArrow.Remove();
			}
		};
		if (Main.LocalPlayer.GetModPlayer<QuestModPlayer>().GetQuestCount() != 0 && _currentQuestIndex != Main.LocalPlayer.GetModPlayer<QuestModPlayer>().GetQuestCount() - 1)
		{
			Append(rightArrow);
		}

		leftArrow.Left.Set(145, 0f);
		leftArrow.Top.Set(-180, 1f);
		leftArrow.Width.Set(128, 0);
		leftArrow.Height.Set(128, 0);
		leftArrow.OnLeftClick += (a, b) =>
		{
			if (_currentQuestIndex == 0)
			{
				return;
			}

			SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
			_currentQuestIndex--;

			if (_currentQuestIndex == 0)
			{
				leftArrow.Remove();
			}

			Append(rightArrow);
		};
		if (_currentQuestIndex != 0)
		{
			Append(leftArrow);
		}
	}

	public override string TabName => "QuestBookMenu";

	public override void Draw(SpriteBatch spriteBatch)
	{
		DrawBack(spriteBatch);
		if (Main.LocalPlayer.GetModPlayer<QuestModPlayer>().GetQuestCount() != 0)
		{
			string name = Main.LocalPlayer.GetModPlayer<QuestModPlayer>().GetQuests()[_currentQuestIndex];
			string text = Main.LocalPlayer.GetModPlayer<QuestModPlayer>().GetQuestSteps(name);
			Utils.DrawBorderStringBig(spriteBatch, text, GetRectangle().Center() + new Vector2(200, -265), Color.White, 0.5f, 0.5f, 0.35f);
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