using PathOfTerraria.Core.Loaders.UILoading;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.PlayerStats;

internal class PlayerStatUIState : DraggableSmartUi
{
	private readonly PlayerStatInnerPanel _mainPanel = new() { Width = StyleDimension.FromPixels(512), Height = StyleDimension.FromPixels(448), HAlign = 0.5f, VAlign = 0.5f };
	public override List<SmartUIElement> TabPanels => [_mainPanel];

	public override int DepthPriority => 2;

	public void Toggle()
	{
		if (IsVisible)
		{
			IsVisible = false;
			return;
		}

		if (!HasChild(_mainPanel))
		{
			Width = StyleDimension.FromPixels(512);
			Height = StyleDimension.FromPixels(448);
			HAlign = 0.5f;
			VAlign = 0.5f;

			RemoveAllChildren();
			Append(_mainPanel);

			CloseButton = new UIImageButton(ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/CloseButton"));
			CloseButton.Left.Set(-10, 0.8f);
			CloseButton.Top.Set(40, 0f);
			CloseButton.Width.Set(38, 0);
			CloseButton.Height.Set(38, 0);
			CloseButton.OnLeftClick += (a, b) =>
			{
				IsVisible = false;
				SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
			};
			CloseButton.SetVisibility(1, 1);
			_mainPanel.Append(CloseButton);
		}

		IsVisible = true;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Recalculate();
		base.Draw(spriteBatch);

		CloseButton.Draw(spriteBatch);
	}
}
