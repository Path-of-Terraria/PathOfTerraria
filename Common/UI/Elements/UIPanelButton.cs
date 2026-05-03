using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Elements;

/// <summary>
///		A simple button: a <see cref="UIPanel"/> with a centered <see cref="UIText"/> label and a
///		click callback. Unlike <see cref="UITextPanel{T}"/>, this control respects the configured
///		<see cref="UIElement.Width"/>/<see cref="UIElement.Height"/> regardless of label length and
///		keeps the label centered.
/// </summary>
public sealed class UIPanelButton : UIPanel
{
	private static readonly Color IdleColor = new Color(63, 82, 151) * 0.85f;
	private static readonly Color HoverColor = new Color(73, 94, 171);

	private readonly UIText _text;
	private readonly Action _onClick;

	public UIPanelButton(LocalizedText label, Action onClick, float textScale = 0.85f)
	{
		_onClick = onClick;

		SetPadding(0);
		BackgroundColor = IdleColor;

		_text = new UIText(label, textScale)
		{
			HAlign = 0.5f,
			VAlign = 0.5f,
		};
		Append(_text);
	}

	public override void LeftClick(UIMouseEvent evt)
	{
		base.LeftClick(evt);
		SoundEngine.PlaySound(SoundID.MenuTick);
		_onClick?.Invoke();
	}

	public override void MouseOver(UIMouseEvent evt)
	{
		base.MouseOver(evt);
		BackgroundColor = HoverColor;
	}

	public override void MouseOut(UIMouseEvent evt)
	{
		base.MouseOut(evt);
		BackgroundColor = IdleColor;
	}
}
