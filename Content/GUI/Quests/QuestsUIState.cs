using System.Collections.Generic;
using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Systems.Questing;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.Quests;

internal class QuestsUIState : DraggableSmartUi
{
	private static readonly Asset<Texture2D> Texture =
		ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/ArrowExtraSmall");

	private readonly QuestDetailsPanel _mainPanel = new()
	{
		Width = StyleDimension.FromPixels(1200),
		Height = StyleDimension.FromPixels(900),
		HAlign = 0.5f,
		VAlign = 0.5f
	};

	private readonly UIImageButton _rightArrow = new(Texture);
	private readonly FlippableUIImageButton _leftArrow = new(Texture) { FlipHorizontally = true };
	private List<Quest> _quests = [];
	public override List<SmartUIElement> TabPanels => [_mainPanel];
	private int currentQuestIndex = 0;

	public override int DepthPriority => 2;

	public void Toggle()
	{
		if (IsVisible)
		{
			IsVisible = false;
			_mainPanel.Remove();
			return;
		}

		if (!HasChild(_mainPanel))
		{
			_mainPanel.Width = StyleDimension.FromPixels(1200);
			_mainPanel.Height = StyleDimension.FromPixels(900);
			_mainPanel.HAlign = 0.5f;
			_mainPanel.VAlign = 0.5f;
			_quests = Main.LocalPlayer.GetModPlayer<QuestModPlayer>().GetAllQuests();
			if (_quests.Count > 0)
			{
				_mainPanel.ViewingQuest = _quests[currentQuestIndex];
			}

			RemoveAllChildren();
			Append(_mainPanel);

			CloseButton =
				new UIImageButton(ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/CloseButton"));
			CloseButton.Left.Set(-200, 1f);
			CloseButton.Top.Set(80, 0f);
			CloseButton.Width.Set(38, 0);
			CloseButton.Height.Set(38, 0);
			CloseButton.OnLeftClick += (a, b) =>
			{
				IsVisible = false;
				SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
			};
			CloseButton.SetVisibility(1, 1);
			_rightArrow.Left.Set(-230, 1f);
			_rightArrow.Top.Set(-180, 1f);
			_rightArrow.Width.Set(128, 0);
			_rightArrow.Height.Set(128, 0);
			_rightArrow.OnLeftClick += (a, b) =>
			{
				if (currentQuestIndex == _quests.Count - 1)
				{
					return;
				}

				SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
				currentQuestIndex++;
				_mainPanel.ViewingQuest = _quests[currentQuestIndex];
			};
			_mainPanel.Append(_rightArrow);
			_mainPanel.Append(CloseButton);

			_leftArrow.Left.Set(145, 0f);
			_leftArrow.Top.Set(-180, 1f);
			_leftArrow.Width.Set(128, 0);
			_leftArrow.Height.Set(128, 0);
			_leftArrow.OnLeftClick += (a, b) =>
			{
				if (currentQuestIndex == 0)
				{
					return;
				}

				SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
				currentQuestIndex--;
				_mainPanel.ViewingQuest = _quests[currentQuestIndex];
			};
			_mainPanel.Append(_leftArrow);
		}

		IsVisible = true;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Recalculate();
		base.Draw(spriteBatch);

		CloseButton.Draw(spriteBatch);
		if (currentQuestIndex != 0)
		{
			_leftArrow.Draw(spriteBatch);
		}

		if (currentQuestIndex != _quests.Count - 1)
		{
			_rightArrow.Draw(spriteBatch);
		}
	}
}

public class FlippableUIImageButton(Asset<Texture2D> texture) : UIImageButton(texture)
{
	private readonly Asset<Texture2D> _texture1 = texture;
	public bool FlipHorizontally { get; set; }

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		if (FlipHorizontally)
		{
			spriteBatch.Draw(_texture1.Value, GetDimensions().ToRectangle(), null, Color.White, 0f, Vector2.Zero,
				SpriteEffects.FlipHorizontally, 0f);
		}
		else
		{
			base.DrawSelf(spriteBatch);
		}
	}
}