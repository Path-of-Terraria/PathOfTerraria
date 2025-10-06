using Terraria.GameContent;

namespace PathOfTerraria.Utilities.Terraria;

internal class ItemDrawing
{
	/// <summary>
	/// Draws an item IN THE WORLD with the associated color and rotation.<br/>
	/// Shorthand for <see cref="DrawSelf(Item, Vector2, Rectangle?, Color, float, Vector2?, float, SpriteEffects)"/>, with the following parameters:<br/>
	/// <c>item.Center, null, baseColor, rotation, null, 1f, SpriteEffects.None</c>
	/// </summary>
	/// <param name="item"></param>
	/// <param name="baseColor"></param>
	/// <param name="rotation"></param>
	public static void DrawSelfQuick(Item item, Color baseColor, float rotation)
	{
		DrawSelf(item, item.Center, null, baseColor, rotation, null, 1f, SpriteEffects.None);
	}

	/// <summary>
	/// Draws an item with functionality for shimmer, <see cref="Item.GetAlpha(Color)"/>, and texture loading if needed.
	/// </summary>
	public static void DrawSelf(Item item, Vector2 position, Rectangle? source, Color baseColor, float rotation, Vector2? origin, float scale, SpriteEffects effects)
	{
		Main.instance.LoadItem(item.type);

		if (item.shimmered)
		{
			rotation = 0;
		}

		Texture2D texture = TextureAssets.Item[item.type].Value;

		origin ??= texture.Size() / 2f;
		Color drawColor = ShimmerUtils.GetShimmeredAlpha(item, baseColor);
		Main.spriteBatch.Draw(texture, position - Main.screenPosition, source, drawColor, rotation, origin.Value, scale, effects, 0);

		if (item.shimmered)
		{
			Main.spriteBatch.Draw(texture, position - Main.screenPosition, source, drawColor with { A = 0 }, rotation, origin.Value, scale, effects, 0);
		}
	}
}
