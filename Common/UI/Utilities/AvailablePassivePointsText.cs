namespace PathOfTerraria.Common.UI.Utilities;

public class AvailablePassivePointsText
{
	public static void DrawAvailablePassivePoint(SpriteBatch spriteBatch, int points, Vector2 position)
	{
		Texture2D tex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/PassiveFrameSmall").Value;
		
		spriteBatch.Draw(tex, position, null, Color.White, 0, tex.Size() / 2f, 1, 0,
			0);
		Utils.DrawBorderStringBig(spriteBatch, $"{points}", position,
			points > 0 ? Color.Yellow : Color.Gray, 0.5f, 0.5f, 0.35f);
		Utils.DrawBorderStringBig(spriteBatch, "Points remaining",
			position + new Vector2(138, 0), Color.White, 0.6f, 0.5f, 0.35f);
	}
}