using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Elements;

/// <summary>
///		A clickable panel with a centered text label that visually reflects an externally-tracked
///		boolean state. Unlike <see cref="UITextPanel{T}"/>, this control respects the explicit
///		<see cref="UIElement.Width"/>/<see cref="UIElement.Height"/> regardless of label length and
///		keeps the label centered both horizontally and vertically.
/// </summary>
public sealed class UIToggleButton : UIPanel
{
	private static readonly Color InactiveColor = new Color(63, 82, 151) * 0.7f;
	private static readonly Color ActiveColor = new Color(126, 168, 73);

	private readonly Func<bool> _getter;
	private readonly Action<bool> _setter;
	private readonly UIText _text;

	public UIToggleButton(LocalizedText label, Func<bool> getter, Action<bool> setter, float textScale = 0.75f)
	{
		_getter = getter;
		_setter = setter;

		SetPadding(0);
		BackgroundColor = InactiveColor;

		_text = new UIText(label, textScale)
		{
			HAlign = 0.5f,
			VAlign = 0.5f,
		};
		Append(_text);
	}

	public void SetText(LocalizedText label)
	{
		_text.SetText(label);
	}

	public override void LeftClick(UIMouseEvent evt)
	{
		base.LeftClick(evt);
		SoundEngine.PlaySound(SoundID.MenuTick);
		_setter(!_getter());
	}

	public override void Update(GameTime gameTime)
	{
		base.Update(gameTime);
		BackgroundColor = _getter() ? ActiveColor : InactiveColor;
	}
}
