using System.Runtime.CompilerServices;
using PathOfTerraria.Core.UI;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

#nullable enable

namespace PathOfTerraria.Common.UI.Components;

/// <summary>
/// Implements hover/click audio, textures, and tooltips.<br/>
/// Supports: UIImage.
/// </summary>
internal sealed class UIInteractive : UIComponent
{
	private bool wasHovered;
	private bool wasPressed;
	private Asset<Texture2D>? defaultTexture;

	public Predicate<UIElement> IsActive { get; set; } = _ => true;
	public Predicate<UIElement> IsUnlocked { get; set; } = _ => true;
	public SoundStyle? HoverSound { get; set; }
	public SoundStyle? ClickSound { get; set; }
	public string? HoverTooltip { get; set; }
	public string? LockedTooltip { get; set; }
	public Asset<Texture2D>? LockedTexture { get; set; }
	public Asset<Texture2D>? HoverTexture { get; set; }
	public Asset<Texture2D>? ClickTexture { get; set; }

	protected override void OnAttach(UIElement element)
	{
		element.OnUpdate += OnUpdate;
		OnUpdate(element);
	}

	protected override void OnDetach(UIElement element)
	{
		element.OnUpdate -= OnUpdate;
	}

	private void OnUpdate(UIElement element)
	{
		bool isActive = IsActive(element);
		bool isLocked = !IsUnlocked(element);
		bool isHovered = Element.IsMouseHovering;
		bool isPressed = isHovered && Main.mouseLeft;
		Asset<Texture2D>? textureOverride = (isLocked ? LockedTexture : null) ?? (isPressed ? ClickTexture : null) ?? (isHovered ? HoverTexture : null);

		// Textures

		switch (element)
		{
			case UIImage img:
			{
				defaultTexture ??= UIImage_GetImage(img);
				img.SetImage(textureOverride ?? defaultTexture);
				break;
			}
		}

		// Audio

		if (isHovered && !wasHovered && isActive && !isLocked)
		{
			SoundEngine.PlaySound(HoverSound);
		}

		if (isPressed && !wasPressed && isActive)
		{
			SoundEngine.PlaySound(ClickSound);
		}

		// Tooltips

		if (isActive && isHovered && ((isLocked ? LockedTooltip : null) ?? HoverTooltip) is string tooltip)
		{
			Main.instance.MouseText(tooltip);
		}

		wasHovered = isHovered && !isLocked;
		wasPressed = isPressed;
	}

	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_texture")]
	static extern ref Asset<Texture2D> UIImage_GetImage(UIImage img);
}
