using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.PlayerStats;

internal class PlayerStatUIState : CloseableSmartUi
{
	public override int DepthPriority => 3;
	public override bool IsCentered => true;

	private PlayerStatInnerPanel statPanel = null;

	public void Toggle()
	{
		if (IsVisible)
		{
			IsVisible = false;
			Panel?.Remove();
			return;
		}

		if (!HasChild(Panel))
		{
			Width = StyleDimension.FromPixels(512);
			Height = StyleDimension.FromPixels(448);
			HAlign = 0.5f;
			VAlign = 0.25f;

			RemoveAllChildren();

			base.CreateMainPanel(false, new Point(512, 448), false, true);

			statPanel = new()
			{
				Width = StyleDimension.FromPixels(512),
				Height = StyleDimension.FromPixels(448),
				HAlign = 0.5f,
				VAlign = 0.5f
			};
			Panel.Append(statPanel);

			CloseButton = new UIImageButton(ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/PlayerStatClose"));
			CloseButton.Left.Set(-40, 0.8f);
			CloseButton.Top.Set(50, 0f);
			CloseButton.Width.Set(64, 0);
			CloseButton.Height.Set(64, 0);
			CloseButton.OnLeftClick += (a, b) =>
			{
				IsVisible = false;
				SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
			};
			CloseButton.SetVisibility(1, 1);
			statPanel.Append(CloseButton);
		}

		IsVisible = true;
		Recalculate();
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		CloseButton.Draw(spriteBatch);
	}
}