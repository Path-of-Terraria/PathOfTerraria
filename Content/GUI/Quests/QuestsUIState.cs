using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using PathOfTerraria.Core.Loaders.UILoading;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.Quests
{
    internal class QuestsUIState : DraggableSmartUi
    {
        private readonly QuestsCompletedInnerPanel _mainPanel = new() { Width = StyleDimension.FromPixels(1200), Height = StyleDimension.FromPixels(900), HAlign = 0.5f, VAlign = 0.5f };
        private readonly UIImageButton _rightArrow = new(ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/ArrowSmall"));
        private readonly FlippableUIImageButton _leftArrow = new(ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/ArrowSmall")) { FlipHorizontally = true };
        public override List<SmartUIElement> TabPanels => new List<SmartUIElement> { _mainPanel };

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
                _rightArrow.Left.Set(-230, 1f);
                _rightArrow.Top.Set(-180, 1f);
                _rightArrow.Width.Set(128, 0);
                _rightArrow.Height.Set(128, 0);
                _rightArrow.OnLeftClick += (a, b) =>
                {
                    SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
                };
                _mainPanel.Append(_rightArrow);
                _mainPanel.Append(CloseButton);

                // Setup the left arrow
                _leftArrow.Left.Set(145, 0f);
                _leftArrow.Top.Set(-180, 1f);
                _leftArrow.Width.Set(128, 0);
                _leftArrow.Height.Set(128, 0);
                _leftArrow.OnLeftClick += (a, b) =>
                {
                    SoundEngine.PlaySound(SoundID.MenuClose, Main.LocalPlayer.Center);
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
            _rightArrow.Draw(spriteBatch);
            _leftArrow.Draw(spriteBatch);
        }
    }
}


public class FlippableUIImageButton(Asset<Texture2D> texture) : UIImageButton(texture)
{
	public bool FlipHorizontally { get; set; }

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		if (FlipHorizontally)
		{
			spriteBatch.Draw(texture.Value, GetDimensions().ToRectangle(), null, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
		}
		else
		{
			base.DrawSelf(spriteBatch);
		}
	}
}