using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;

namespace PathOfTerraria.Common.UI.Elements;

/// <summary>
///     Provides a <see cref="UIImage" /> that displays a tooltip on hover
///     and contains hover transform effects.
/// </summary>
public class UIHoverTooltipImage : UIImage
{
	/// <summary>
	///     The target rotation for this image when it's being hovered by the mouse.
	/// </summary>
	/// <remarks>
	///     Defaults to <c>0f</c>.
	/// </remarks>
	public float ActiveRotation = 0f;

	/// <summary>
	///     The target scale for this image when it's being hovered by the mouse.
	/// </summary>
	/// <remarks>
	///     Defaults to <c>1f</c>.
	/// </remarks>
	public float ActiveScale = 1f;

	/// <summary>
	///     The target rotation for this image when it's not being hovered by the mouse.
	/// </summary>
	/// <remarks>
	///     Defaults to <c>0f</c>.
	/// </remarks>
	public float InactiveRotation = 0f;

	/// <summary>
	///     The target scale for this image when it's not being hovered by the mouse.
	/// </summary>
	/// <remarks>
	///     Defaults to <c>1f</c>.
	/// </remarks>
	public float InactiveScale = 1f;

	/// <summary>
	///     The localization key for the image's tooltip.
	/// </summary>
	public string Key;

	/// <summary>
	///     The smoothness used to perform scale/rotation interpolations.
	/// </summary>
	/// <remarks>
	///     Defaults to <c>0.3f</c>. Ranges from <c>0f</c> - <c>1f</c>.
	/// </remarks>
	public float Smoothness = 0.3f;

	public UIHoverTooltipImage(Asset<Texture2D> texture, string key) : base(texture)
	{
		Key = key;
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		Rotation = MathHelper.SmoothStep(Rotation, IsMouseHovering ? ActiveRotation : InactiveRotation, Smoothness);
		ImageScale = MathHelper.SmoothStep(ImageScale, IsMouseHovering ? ActiveScale : InactiveScale, Smoothness);

		if (!IsMouseHovering)
		{
			return;
		}

		Main.instance.MouseText(Language.GetTextValue(Key));
	}
}