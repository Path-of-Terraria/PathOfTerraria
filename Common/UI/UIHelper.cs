namespace PathOfTerraria.Common.UI;

internal static class UIHelper
{
	/// <summary>
	/// Gets the position of a inventory button, and returns if it's being hovered over. Assumes a 64x64 clickbox.
	/// </summary>
	/// <param name="y">Y position of the button.</param>
	/// <param name="pos">The resulting position of the button.</param>
	/// <returns>If the button is being hovered over.</returns>
	public static bool GetInvButtonInfo(int y, out Vector2 pos)
	{
		pos = new(GetTextureXPosition(), y);
		var bounds = new Rectangle((int)(pos.X - 64 / 1.125f), (int)pos.Y, 64, 64);
		return bounds.Contains(Main.MouseScreen.ToPoint());
	}

	/// <summary>
	/// Adjusts the X position of a minimap-aligned inventory button based on screen width.
	/// </summary>
	/// <returns>X position of the button.</returns>
	private static float GetTextureXPosition()
	{
		float screenWidth = Main.screenWidth / 1.12f;
		return screenWidth switch
		{
			//4k or 4k Wide
			>= 2160 => screenWidth / 1.001f,
			//1440p+
			>= 1440 => screenWidth / 1.06f,
			_ => screenWidth / 1.125f
		};
	}
}
