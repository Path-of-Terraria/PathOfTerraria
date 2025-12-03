using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.UI.Utilities;

public class AvailablePassivePointsText
{
	public static void DrawResettablePoints(SpriteBatch spriteBatch, int points, Vector2 position, ref int confirmTimer, Action onReset)
	{
		DrawAvailablePassivePoint(spriteBatch, points, position);

		string text = Language.GetTextValue("Mods.PathOfTerraria.UI." + (confirmTimer <= 0 ? "ResetPoints" : "Confirm"));
		Vector2 size = ChatManager.GetStringSize(FontAssets.DeathText.Value, text, new(0.6f));
		size.Y = 28; // For some reason the string size above is twice the height
		Vector2 resetPosition = position + new Vector2(-20, 30);
		bool hover = new Rectangle((int)resetPosition.X, (int)resetPosition.Y, (int)size.X, (int)size.Y).Contains(Main.MouseScreen.ToPoint());
		Color col = hover ? Color.Gray : Color.White;

		if (confirmTimer > 0)
		{
			col = hover ? Color.DarkRed : Color.Red;
		}

		Utils.DrawBorderStringBig(spriteBatch, text, resetPosition, col, 0.6f, 0f, 0f);

		if (hover && Main.mouseLeft && Main.mouseLeftRelease)
		{
			Main.mouseLeftRelease = false;

			if (confirmTimer <= 0)
			{
				confirmTimer = 120;
			}
			else
			{
				confirmTimer = 0;
				onReset();
			}
		}
	}

	public static void DrawAvailablePassivePoint(SpriteBatch spriteBatch, int points, Vector2 position)
	{
		Texture2D tex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/PassiveFrameSmall").Value;

		spriteBatch.Draw(tex, position, null, Color.White, 0, tex.Size() / 2f, 1, 0, 0);
		Utils.DrawBorderStringBig(spriteBatch, $"{points}", position, points > 0 ? Color.Yellow : Color.Gray, 0.5f, 0.5f, 0.35f);
		Utils.DrawBorderStringBig(spriteBatch, Language.GetTextValue("Mods.PathOfTerraria.UI.PointsRemaining"),
			position + new Vector2(138, 0), Color.White, 0.6f, 0.5f, 0.35f);
	}
}