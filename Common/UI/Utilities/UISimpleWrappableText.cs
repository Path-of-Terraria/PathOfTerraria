using System.Text;
using ReLogic.Graphics;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.UI.Utilities;

// ReSharper disable once InconsistentNaming
public class UISimpleWrappableText : UIElement
{
	private TextSnippet[] _array;
	private string _pageText;

	private bool _centered;
	private bool _wrappable;
	private Color _colour;

	public bool Centered
	{
		get => _centered;
		set
		{
			_centered = value;
			UpdateText();
		}
	}

	public Color Colour
	{
		get => _colour;
		set
		{
			_colour = value;
			UpdateText();
		}
	}

	private float Scale
	{
		get => scale;
		set
		{
			scale = value;
			UpdateText();
		}
	}

	private int MaxLines { get; set; } = 99;
	public bool Border { get; set; }

	public int Page
	{
		get => page;
		set
		{
			page = value;
			UpdateText();
		}
	}

	private int MaxPage { get; set; }

	public bool Wrappable
	{
		get => _wrappable;
		set
		{
			_wrappable = value;
			UpdateText();
		}
	}

	public Color BorderColour { get; set; }

	private DynamicSpriteFont Font => FontAssets.MouseText.Value;

	private float _drawOffsetX;
	private int page;
	private float scale;
	private string _text;

	public UISimpleWrappableText(string text, float textScale = 1f)
	{
		_text = text;
		_colour = Color.White;
		Scale = textScale;

		UpdateText();
	}

	public void SetText(string text)
	{
		_text = text;

		UpdateText();
	}

	private void UpdateText()
	{
		if (string.IsNullOrEmpty(_text))
		{
			_array = null;
			return;
		}

		string[] lines = Wrappable
			? WrapText(Font, _text, GetDimensions().Width, Scale).Split('\n')
			: _text.Split('\n');
		MaxPage = (lines.Length - 1) / MaxLines;
		if (MaxPage < 0)
		{
			MaxPage = 0;
		}

		// add the right lines to the page
		if (lines.Length > MaxLines)
		{
			int startLine = MaxLines * Page;
			var builder = new StringBuilder();
			for (int i = startLine; i < startLine + MaxLines && i < lines.Length; i++)
			{
				builder.AppendLine(lines[i]);
			}

			_pageText = builder.ToString();
		}
		else
		{
			_pageText = _text;
		}

		_array = ChatManager.ParseMessage(_pageText, Colour).ToArray();
		ChatManager.ConvertNormalSnippets(_array);

		if (Centered)
		{
			_drawOffsetX =
				ChatManager.GetStringSize(Font, _array, new Vector2(Scale), Wrappable ? GetDimensions().Width : -1f).X *
				0.5f;
		}
	}

	public static string WrapText(DynamicSpriteFont font, string text, float maxLineWidth, float fontScale = 1f)
	{
		if (string.IsNullOrEmpty(text))
		{
			return "";
		}

		string[] lines = text.Split('\n');
		string newText = "";
		float currentWidth = 0f;
		float spaceWidth = font.MeasureString(" ").X * fontScale;

		for (int i = 0; i < lines.Length; i++)
		{
			string line = lines[i];
			string[] words = line.Split(' ');
			foreach (string word in words)
			{
				Vector2 wordSize = font.MeasureString(word) * fontScale;

				if (currentWidth + wordSize.X < maxLineWidth)
				{
					newText += word + " ";
					currentWidth += wordSize.X + spaceWidth;
					continue;
				}

				newText += Environment.NewLine + word + " ";
				currentWidth = wordSize.X + spaceWidth;
			}

			currentWidth = 0f;
			if (i < lines.Length - 1)
			{
				newText += "\n";
			}
		}

		return newText;
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		if (_array == null)
		{
			// no text to draw
			return;
		}

		// get positions
		CalculatedStyle style = GetDimensions();
		Vector2 tl = style.Position();
		if (Centered)
		{
			tl = new Vector2(style.Center().X, tl.Y);
		}

		Vector2 pos = tl - Vector2.UnitX * _drawOffsetX;

		float maxWidth = Wrappable ? style.Width : -1f;

		// draw
		if (Border)
		{
			DrawColorCodedStringShadow(spriteBatch, Font, _array, pos, BorderColour, 0f, Vector2.Zero,
				new Vector2(Scale), maxWidth);
		}

		DrawColorCodedString(spriteBatch, Font, _array, pos, Colour, 0f, Vector2.Zero, new Vector2(Scale), out int h,
			maxWidth);
	}

	private static void DrawColorCodedStringShadow(SpriteBatch spriteBatch, DynamicSpriteFont font,
		TextSnippet[] snippets, Vector2 position, Color baseColor, float rotation, Vector2 origin, Vector2 baseScale,
		float maxWidth = -1f, float spread = 2f)
	{
		foreach (Vector2 t in ChatManager.ShadowDirections)
		{
			DrawColorCodedString(spriteBatch, font, snippets, position + t * spread,
				baseColor, rotation, origin, baseScale, out int _, maxWidth, true);
		}
	}

	private static void DrawColorCodedString(SpriteBatch spriteBatch, DynamicSpriteFont font, TextSnippet[] snippets,
		Vector2 position, Color baseColor, float rotation, Vector2 origin, Vector2 baseScale, out int hoveredSnippet,
		float maxWidth, bool ignoreColors = false)
	{
		int num = -1;
		var vector21 = new Vector2(Main.mouseX, Main.mouseY);
		Vector2 x = position;
		Vector2 vector22 = x;
		float single = font.MeasureString(" ").X;
		Color visibleColor = baseColor;
		float single1 = 0f;
		for (int i = 0; i < snippets.Length; i++)
		{
			TextSnippet textSnippet = snippets[i];
			textSnippet.Update();
			if (!ignoreColors)
			{
				visibleColor = textSnippet.GetVisibleColor();
			}

			float scale = textSnippet.Scale;
			if (!textSnippet.UniqueDraw(false, out Vector2 vector2, spriteBatch, x, visibleColor, scale))
			{
				string[] strArrays = textSnippet.Text.Split(['\n']);
				string[] strArrays1 = strArrays;
				for (int j = 0; j < strArrays1.Length; j++)
				{
					string[] strArrays2 = strArrays1[j].Split([' ']);
					for (int k = 0; k < strArrays2.Length; k++)
					{
						if (k != 0)
						{
							ref float singlePointer = ref x.X;
							singlePointer += single * baseScale.X * scale;
						}

						if (maxWidth > 0f &&
						    x.X - position.X + font.MeasureString(strArrays2[k]).X * baseScale.X * scale > maxWidth)
						{
							x.X = position.X;
							ref float y = ref x.Y;
							y += font.LineSpacing * single1 * baseScale.Y;
							vector22.Y = Math.Max(vector22.Y, x.Y);
							single1 = 0f;
						}

						if (single1 < scale)
						{
							single1 = scale;
						}

						DynamicSpriteFontExtensionMethods.DrawString(spriteBatch, font, strArrays2[k], x, visibleColor,
							rotation, origin, baseScale * textSnippet.Scale * scale, SpriteEffects.None, 0f);
						Vector2 vector23 = font.MeasureString(strArrays2[k]);
						if (vector21.Between(x, x + vector23))
						{
							num = i;
						}

						ref float x1 = ref x.X;
						x1 += vector23.X * baseScale.X * scale;
						vector22.X = Math.Max(vector22.X, x.X);
					}

					if (strArrays.Length <= 1 || j >= strArrays1.Length - 1)
					{
						continue;
					}

					ref float lineSpacing = ref x.Y;
					lineSpacing += font.LineSpacing * single1 * baseScale.Y;
					x.X = position.X;
					vector22.Y = Math.Max(vector22.Y, x.Y);
					single1 = 0f;
				}
			}
			else
			{
				if (vector21.Between(x, x + vector2))
				{
					num = i;
				}

				ref float singlePointer1 = ref x.X;
				singlePointer1 += vector2.X * baseScale.X * scale;
				vector22.X = Math.Max(vector22.X, x.X);
			}
		}

		hoveredSnippet = num;
	}
}