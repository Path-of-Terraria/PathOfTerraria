using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;

namespace PathOfTerraria.Common.UI.Elements;

/// <summary>
///     Provides a <see cref="UIImage" /> that displays a tooltip on hover.
/// </summary>
public class UITooltipImage : UIImage
{
	/// <summary>
	///     The localization key for the image's tooltip.
	/// </summary>
	public string Key;

	public UITooltipImage(Asset<Texture2D> texture, string key) : base(texture)
	{
		Key = key;
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		if (!IsMouseHovering)
		{
			return;
		}

		Main.instance.MouseText(Language.GetTextValue(Key));
	}
}