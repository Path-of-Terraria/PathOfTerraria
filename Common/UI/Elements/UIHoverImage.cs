using ReLogic.Content;
using Terraria.GameContent.UI.Elements;

namespace PathOfTerraria.Common.UI.Elements;

/// <summary>
///     Provides a <see cref="UIImage" /> that contains hover transform effects.
/// </summary>
public class UIHoverImage : UIImage
{
	/// <summary>
	///     The target rotation for this image when it's being hovered by the mouse, in radians. Defaults to <c>0f</c>.
	/// </summary>
	public float ActiveRotation = 0f;

	/// <summary>
	///     The target scale for this image when it's being hovered by the mouse, in percentage. Defaults to <c>1f</c>.
	/// </summary>
	public float ActiveScale = 1f;

	/// <summary>
	///     The target rotation for this image when it's not being hovered by the mouse, in radians. Defaults to <c>0f</c>.
	/// </summary>
	public float InactiveRotation = 0f;

	/// <summary>
	///     The target scale for this image when it's not being hovered by the mouse, in percentage. Defaults to <c>1f</c>.
	/// </summary>
	public float InactiveScale = 1f;

	/// <summary>
	///     The smoothness used to perform transform interpolations. Defaults to <c>0.3f</c>. Ranges from <c>0f</c> - <c>1f</c>.
	/// </summary>
	public float Smoothness
	{
		get => smoothness;
		set => smoothness = MathHelper.Clamp(value, 0f, 1f);
	}

	private float smoothness = 0.3f;

	public UIHoverImage(Asset<Texture2D> texture) : base(texture) { }

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		Rotation = MathHelper.SmoothStep(Rotation, IsMouseHovering ? ActiveRotation : InactiveRotation, Smoothness);
		ImageScale = MathHelper.SmoothStep(ImageScale, IsMouseHovering ? ActiveScale : InactiveScale, Smoothness);
	}
}