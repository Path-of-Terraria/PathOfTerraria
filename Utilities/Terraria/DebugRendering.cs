using Terraria.GameContent;

namespace PathOfTerraria.Utilities.Terraria;

internal static class DebugRendering
{
	/// <summary> Immediately renders a simple circle out of lines. Not very performant. </summary>
	public static void DrawCircle(SpriteBatch sb, Vector2 center, float radius, Color color, int resolution = 16, int width = 2)
	{
		float step = MathHelper.TwoPi / resolution;
		var offset = new Vector2(radius, 0f);

		for (int i = 0; i <= resolution; i++)
		{
			Vector2 lineStart = center + offset;
			Vector2 lineEnd = center + (offset = offset.RotatedBy(step));
			Vector2 startEndDelta = lineEnd - lineStart;

			var rect = new Rectangle(
				(int)lineStart.X,
				(int)lineStart.Y,
				(int)startEndDelta.Length(),
				width
			);

			sb.Draw(TextureAssets.BlackTile.Value, rect, null, color, startEndDelta.ToRotation(), Vector2.Zero, SpriteEffects.None, 0f);
		}
	}
}
