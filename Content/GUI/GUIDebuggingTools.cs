using PathOfTerraria.Config;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI;

// ReSharper disable once InconsistentNaming
public class GUIDebuggingTools
{
	/// <summary>
	/// Draws a border around a UIElements for showing the draw area
	/// </summary>
	/// <param name="spriteBatch"></param>
	/// <param name="dimensions"></param>
	/// <param name="borderColor"></param>
	public static void DrawGuiBorder(SpriteBatch spriteBatch, CalculatedStyle dimensions, Color? borderColor)
	{
		if (!ModContent.GetInstance<DeveloperConfig>().DrawGuiBorders)
		{
			return;
		}

		borderColor ??= Color.Red;
		int borderThickness = 2;

		var pixelTexture = new Texture2D(Main.graphics.GraphicsDevice, 1, 1);
		pixelTexture.SetData([Color.White]);
		// Draw the top border
		spriteBatch.Draw(pixelTexture, new Rectangle((int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width, borderThickness), (Color) borderColor);
		// Draw the bottom border
		spriteBatch.Draw(pixelTexture, new Rectangle((int)dimensions.X, (int)(dimensions.Y + dimensions.Height - borderThickness), (int)dimensions.Width, borderThickness), (Color) borderColor);
		// Draw the left border
		spriteBatch.Draw(pixelTexture, new Rectangle((int)dimensions.X, (int)dimensions.Y, borderThickness, (int)dimensions.Height), (Color) borderColor);
		// Draw the right border
		spriteBatch.Draw(pixelTexture, new Rectangle((int)(dimensions.X + dimensions.Width - borderThickness), (int)dimensions.Y, borderThickness, (int)dimensions.Height), (Color) borderColor);
	}
}