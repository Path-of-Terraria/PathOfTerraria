using ReLogic.Graphics;
using System.Collections.Generic;

namespace PathOfTerraria.Common.Utilities;

public static class StringUtils
{
	/// <summary>
	/// Wraps a string to a given maximum width, by forcibly adding newlines. Normal newlines will be removed, put the text 'NEWBLOCK' in your string to break a paragraph if needed.
	/// </summary>
	/// <param name="input">The input string to be wrapped</param>
	/// <param name="length">The maximum width of the text</param>
	/// <param name="font">The font the text will be drawn in, to calculate its size</param>
	/// <param name="scale">The scale the text will be drawn at, to calculate its size</param>
	/// <returns>Input text with linebreaks inserted so it obeys the width constraint.</returns>
	public static string WrapString(string input, int length, DynamicSpriteFont font, float scale)
	{
		string output = "";

		// In case input is empty and causes an error, we put an empty string to the list
		var words = new List<string> { "" };

		// Word splitting, with CJK characters being treated as a single word
		string cacheString = "";
		for (int i = 0; i < input.Length; i++)
		{
			// By doing this we split words, and make the first character of words always a space
			if (cacheString != string.Empty && char.IsWhiteSpace(input[i]))
			{
				words.Add(cacheString);
				cacheString = "";
			}

			// Single CJK character just get directly added to the list
			if (LocalizationUtils.IsCjkCharacter(input[i]))
			{
				if (cacheString != string.Empty)
				{
					words.Add(cacheString);
					cacheString = "";
				}

				// If the next character is a CJK punctuation, we add both characters as a single word
				// Unless the next character is a right close CJK punctuation (e.g. left brackets), in which case we add only the current character
				if (i + 1 < input.Length && LocalizationUtils.IsCjkPunctuation(input[i + 1]) &&
					!LocalizationUtils.IsRightCloseCjkPunctuation(input[i + 1]))
				{
					words.Add(input[i].ToString() + input[i + 1]);
					i++;
				}
				else
				{
					words.Add(input[i].ToString());
				}

				continue;
			}

			cacheString += input[i];
		}

		// Add the last word
		if (!string.IsNullOrEmpty(cacheString))
		{
			words.Add(cacheString);
		}

		string line = "";
		foreach (string str in words)
		{
			if (str == " NEWBLOCK")
			{
				output += "\n\n";
				line = "";
				continue;
			}

			if (str == " NEWLN")
			{
				output += "\n";
				line = "";
				continue;
			}

			if (font.MeasureString(line).X * scale < length)
			{
				output += str;
				line += str;
			}
			else
			{
				// We don't want the first character of a line to be a space
				output += "\n" + str.TrimStart();
				line = str;
			}
		}

		return output;
	}

	/// <summary>
	/// Gets the size of a texture - The mod name is automatically prepended to the texture path.
	/// </summary>
	/// <param name="texturePath"></param>
	/// <returns></returns>
	public static Vector2? GetSizeOfTexture(string texturePath)
	{
		if (!ModContent.HasAsset($"{PoTMod.ModName}/{texturePath}"))
		{
			return null;
		}

		Texture2D tex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/{texturePath}",
			ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
		return tex.Size();
	}
}