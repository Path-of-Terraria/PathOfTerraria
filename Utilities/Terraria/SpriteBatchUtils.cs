using ReLogic.Graphics;

#nullable enable

namespace PathOfTerraria.Utilities.Terraria;

internal static class SpriteBatchUtils
{
	public static void DrawStringOutlined(this SpriteBatch sb, DynamicSpriteFont font, string text, Vector2 position, Color color, Vector2 origin = default, Vector2? scale = null, Color? outlineColor = null)
	{
		Color newColor = outlineColor ?? Color.Black with { A = 128 };

		scale ??= Vector2.One;

		for (int i = 0; i < 5; i++)
		{
			if (i == 4) { newColor = color; }
			
			Vector2 offset = i switch
			{
				0 => new Vector2(-2f, +0f),
				1 => new Vector2(+2f, +0f),
				2 => new Vector2(+0f, -2f),
				3 => new Vector2(+0f, +2f),
				_ => default,
			};

			sb.DrawString(font, text, position + offset, newColor, 0f, origin, scale.Value, SpriteEffects.None, 0f);
		}
	}
}
