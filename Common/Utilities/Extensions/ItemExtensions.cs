using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace PathOfTerraria.Common.Utilities.Extensions;

/// <summary>
///		Provides basic <see cref="Item"/> extension methods.
/// </summary>
public static class ItemExtensions
{
	/// <summary>
	///		Whether the item is a weapon or not.
	/// </summary>
	/// <param name="item">The item to check.</param>
	/// <returns><c>true</c> if the item is a weapon; otherwise, <c>false</c>.</returns>
	public static bool IsWeapon(this Item item)
	{
		return item.damage > 0 && item.pick <= 0;
	}

	public static Color ShimmerizeColor(this Item item, Color baseColor)
	{
		if (!item.shimmered)
		{
			return baseColor;
		}

		baseColor.R = (byte)(baseColor.R * (1f - item.shimmerTime));
		baseColor.G = (byte)(baseColor.G * (1f - item.shimmerTime));
		baseColor.B = (byte)(baseColor.B * (1f - item.shimmerTime));
		baseColor.A = (byte)(baseColor.A * (1f - item.shimmerTime));
		return baseColor;
	}

	public static Color GetShimmeredAlpha(this Item item, Color baseColor)
	{
		return item.ShimmerizeColor(item.GetAlpha(baseColor));
	}

	/// <summary>
	/// Draws an item IN THE WORLD with the associated color and rotation.<br/>
	/// Shorthand for <see cref="DrawSelf(Item, Vector2, Rectangle?, Color, float, Vector2?, float, SpriteEffects)"/>, with the following parameters:<br/>
	/// <c>item.Center, null, baseColor, rotation, null, 1f, SpriteEffects.None</c>
	/// </summary>
	/// <param name="item"></param>
	/// <param name="baseColor"></param>
	/// <param name="rotation"></param>
	public static void DrawSelfQuick(this Item item, Color baseColor, float rotation)
	{
		item.DrawSelf(item.Center, null, baseColor, rotation, null, 1f, SpriteEffects.None);
	}

	/// <summary>
	/// Draws an item with functionality for shimmer, <see cref="Item.GetAlpha(Color)"/>, and texture loading if needed.
	/// </summary>
	public static void DrawSelf(this Item item, Vector2 position, Rectangle? source, Color baseColor, float rotation, Vector2? origin, float scale, SpriteEffects effects)
	{
		Main.instance.LoadItem(item.type);

		if (item.shimmered)
		{
			rotation = 0;
		}

		Texture2D texture = TextureAssets.Item[item.type].Value;

		origin ??= texture.Size() / 2f;
		Color drawColor = item.GetShimmeredAlpha(baseColor);
		Main.spriteBatch.Draw(texture, position - Main.screenPosition, source, drawColor, rotation, origin.Value, scale, effects, 0);

		if (item.shimmered)
		{
			Main.spriteBatch.Draw(texture, position - Main.screenPosition, source, drawColor with { A = 0 }, rotation, origin.Value, scale, effects, 0);
		}
	}
}