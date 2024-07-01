using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.Utilities;

public class UiFlippableImageButton(Asset<Texture2D> texture, UIElement element) : UIImageButton(texture)
{
	private readonly Asset<Texture2D> _texture1 = texture;
	public bool FlipHorizontally { get; set; }

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		if (FlipHorizontally)
		{
			spriteBatch.Draw(_texture1.Value, element.GetDimensions().ToRectangle(), null, Color.White, 0f, Vector2.Zero,
				SpriteEffects.FlipHorizontally, 0f);
		}
		else
		{
			base.DrawSelf(spriteBatch);
		}
	}
}