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
	private QuestDetailsPanel _mainPanel;
	public override List<SmartUIElement> TabPanels => [_mainPanel];

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
			RemoveAllChildren();

			_mainPanel = new QuestDetailsPanel
			{
				Width = StyleDimension.FromPixels(1200),
				Height = StyleDimension.FromPixels(900),
				HAlign = 0.5f,
				VAlign = 0.5f
			};
			Width = StyleDimension.FromPixels(1200);
			Height = StyleDimension.FromPixels(900);
			HAlign = 0.5f;
			VAlign = 0.5f;

			Append(_mainPanel);

			CloseButton = new UIImageButton(ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/CloseButton"));
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

			Append(CloseButton);
		}

		IsVisible = true;
	}
}