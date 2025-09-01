using ReLogic.Localization.IME;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.UI;
using ReLogic.OS;

namespace PathOfTerraria.Common.UI.Elements;

public enum InputType
{
	Text,
	Integer,
	Number
}

/// <summary>
/// Ported from DragonLens: https://github.com/ScalarVector1/DragonLens/blob/master/Content/GUI/FieldEditors/TextField.cs <br/>
/// Cleaned up and modified to not use DragonLens UI.
/// </summary>
public partial class UIEditableText(InputType inputType = InputType.Text, string backingText = "", int maxChars = -1, bool leftAligned = true) : UIElement
{
	public readonly InputType InputType = inputType;

	private readonly string _backingString = backingText;
	private readonly int _maxChars = maxChars;
	private readonly bool _leftAligned = leftAligned;

	public string UseValue => CurrentValue == string.Empty ? _backingString : CurrentValue;

	public string CurrentValue = "";

	private bool _typing;
	private bool _updated;
	private bool _reset;

	// Composition string is handled at the very beginning of the update
	// In order to check if there is a composition string before backspace is typed, we need to check the previous state
	private bool _oldHasCompositionString;

	public void SetTyping()
	{
		_typing = true;
		Main.blockInput = true;
	}

	public void SetNotTyping()
	{
		_typing = false;
		Main.blockInput = false;
	}

	public override void LeftClick(UIMouseEvent evt)
	{
		SetTyping();
	}

	public override void RightClick(UIMouseEvent evt)
	{
		SetTyping();
		CurrentValue = "";
		_updated = true;
	}

	public override void Update(GameTime gameTime)
	{
		if (_reset)
		{
			_updated = false;
			_reset = false;
		}

		if (_updated)
		{
			_reset = true;
		}

		if (Main.mouseLeft && !IsMouseHovering)
		{
			SetNotTyping();
		}
	}

	public void HandleText()
	{
		if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
		{
			SetNotTyping();
		}

		PlayerInput.WritingText = true;
		Main.instance.HandleIME();

		string newText = Main.GetInputText(CurrentValue);

		if (_maxChars != -1 && newText.Length > _maxChars) // Cap input string
		{
			newText = newText[.._maxChars];
		}

		// GetInputText() handles typing operation, but there is a issue that it doesn't handle backspace correctly when the composition string is not empty.
		// It will delete a character both in the text and the composition string instead of only the one in composition string.
		// We'll fix the issue here to provide a better user experience
		if (_oldHasCompositionString && Main.inputText.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Back))
		{
			newText = CurrentValue; // force text not to be changed
		}

		if (newText != CurrentValue)
		{
			if (InputType == InputType.Integer)
			{
				if (newText == string.Empty || int.TryParse(newText, out _))
				{
					CurrentValue = newText;
					_updated = true;
				}
			}
			else if (InputType == InputType.Number) //I found this regex on SO so no idea if it works right lol
			{
				if (newText == string.Empty || float.TryParse(newText, out _))
				{
					CurrentValue = newText;
					_updated = true;
				}
			}
			else
			{
				CurrentValue = newText;
				_updated = true;
			}
		}

		_oldHasCompositionString = Platform.Get<IImeService>().CompositionString is { Length: > 0 };
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		const float Scale = 1f;

		if (_typing)
		{
			HandleText();

			// Draw ime panel, note that if there's no composition string then it won't draw anything
			Main.instance.DrawWindowsIMEPanel(GetDimensions().Position());
		}

		var rect = GetDimensions().ToRectangle();
		Vector2 pos = GetDimensions().Position() + Vector2.One * 4;
		Color color = Color.White;
		float dir = _leftAligned ? 1 : 0;

		if (!_leftAligned)
		{
			pos.X += rect.Width;
		}

		if (rect.Contains(Main.MouseScreen.ToPoint()))
		{
			color = new Color(180, 180, 180);
		}

		string displayed = CurrentValue ?? "";

		if (displayed != string.Empty)
		{
			Utils.DrawBorderString(spriteBatch, displayed, pos, color, Scale, _leftAligned ? 0 : 1);
		}
		else
		{
			Utils.DrawBorderString(spriteBatch, _backingString, pos, Color.Gray.MultiplyRGB(color), Scale, _leftAligned ? 0 : 1);
		}

		// Composition string + cursor drawing below
		if (!_typing)
		{
			return;
		}

		pos.X += FontAssets.MouseText.Value.MeasureString(displayed).X * Scale * dir;
		string compositionString = Platform.Get<IImeService>().CompositionString;

		if (compositionString is { Length: > 0 })
		{
			Utils.DrawBorderString(spriteBatch, compositionString, pos, new Color(255, 240, 20), Scale, _leftAligned ? 0 : 1);
			pos.X += FontAssets.MouseText.Value.MeasureString(compositionString).X * Scale * dir;
		}

		if (Main.GameUpdateCount % 18 < 9)
		{
			if (!_leftAligned)
			{
				pos.X += 6;
			}

			Utils.DrawBorderString(spriteBatch, "|", pos, Color.White, Scale, _leftAligned ? 0 : 1);
		}
	}
}