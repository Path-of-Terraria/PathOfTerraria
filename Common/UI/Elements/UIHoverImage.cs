using System.Runtime.CompilerServices;
using PathOfTerraria.Utilities;
using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

#nullable enable

namespace PathOfTerraria.Common.UI.Elements;

/// <summary>
///     Provides a <see cref="UIImage" /> that contains hover transform effects.
/// </summary>
public class UIHoverImage(Asset<Texture2D> texture, Asset<Texture2D>? hoverTexture = null) : UIImage(texture)
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

	/// <summary> The texture use as an override for this imsage when it's being hovered by the mouse. </summary>
	public Asset<Texture2D>? HoverTexture { get; set; } = hoverTexture;

	/// <summary>
	///     The smoothness used to perform transform interpolations. Defaults to <c>0.3f</c>. Ranges from <c>0f</c> - <c>1f</c>.
	/// </summary>
	public float Smoothness
	{
		get => smoothness;
		set => smoothness = MathHelper.Clamp(value, 0f, 1f);
	}

	/// <summary>
	/// Item to show in place of the standard image.
	/// </summary>
	private Item? item;
	private float smoothness = 0.3f;

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);

		Rotation = MathHelper.SmoothStep(Rotation, IsMouseHovering ? ActiveRotation : InactiveRotation, Smoothness);
		ImageScale = MathHelper.SmoothStep(ImageScale, IsMouseHovering ? ActiveScale : InactiveScale, Smoothness);
	}

	public void SetItem(Item item)
	{
		this.item = item;
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		if (item is null || item.IsAir)
		{
			// Temporary override for the used texture.
			[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_nonReloadingTexture")]
			static extern ref Texture2D NonReloadingTexture(UIImage self);
			using ValueOverride<Texture2D> _ = IsMouseHovering && HoverTexture != null ? ValueOverride.Create(ref NonReloadingTexture(this), HoverTexture.Value) : default;

			base.DrawSelf(spriteBatch);
		}
		else
		{
			CalculatedStyle dimensions = Parent.GetDimensions();
			Vector2 size = new(dimensions.Width, dimensions.Height);
			Vector2 position = dimensions.Position() + size / 2f + size * NormalizedOrigin;

			if (RemoveFloatingPointsFromDrawPosition)
			{
				position = position.Floor();
			}

			ItemSlot.DrawItemIcon(item, ItemSlot.Context.InventoryItem, spriteBatch, position, 1f * ImageScale, 24f * ImageScale, Color.White);
		}
	}
}