using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.Buffs;

#nullable enable

internal static class BuffUtils
{
	public delegate void PreDrawBuffNumberDelegate(BuffDrawParams drawParams, ref Vector2 position, ref Color color, ref float scale, ref string text);

	public static void DrawNumberOverBuff(BuffDrawParams drawParams, string chargeText, PreDrawBuffNumberDelegate? preDrawBuff = null)
	{
		Vector2 textSize = FontAssets.MouseText.Value.MeasureString(chargeText);
		Color color = Color.White;
		float scale = 1f;

		//Top middle of buff icon
		Vector2 textPosition = new(
			drawParams.Position.X + 16,
			drawParams.Position.Y + 4
		);

		preDrawBuff?.Invoke(drawParams, ref textPosition, ref color, ref scale, ref chargeText);
		ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, chargeText, textPosition, color, 0f, textSize / 2f, new Vector2(scale));
	}
}
