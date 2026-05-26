using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Tiles;
using SubworldLibrary;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Visuals;

internal interface ISpecialGlowTile
{
	public Color GlowColor => new(255, 220, 120);
}

internal sealed class SpecialObjectGlowItem : GlobalItem
{
	private const float GlowOpacity = 0.28f;

	public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
	{
		if (!ShouldGlow(item))
		{
			return;
		}

		Texture2D texture = TextureAssets.Item[item.type].Value;
		Rectangle frame = Main.itemAnimations[item.type]?.GetFrame(texture) ?? texture.Frame();
		Vector2 origin = frame.Size() * 0.5f;
		Vector2 position = item.Center - Main.screenPosition;
		Color glowColor = GetItemGlowColor(item) * (GlowOpacity * Pulse(whoAmI));

		DrawGlow(spriteBatch, texture, position, frame, glowColor, rotation, origin, scale);
		Lighting.AddLight(item.Center, glowColor.ToVector3() * 0.35f);
	}

	private static bool ShouldGlow(Item item)
	{
		if (item.IsAir)
		{
			return false;
		}

		if (item.questItem || item.rare == ItemRarityID.Quest)
		{
			return true;
		}

		string itemNamespace = item.ModItem?.GetType().Namespace;
		return itemNamespace is "PathOfTerraria.Content.Items.Quest" or "PathOfTerraria.Content.Items.BossDomain";
	}

	private static Color GetItemGlowColor(Item item)
	{
		string itemNamespace = item.ModItem?.GetType().Namespace;
		return itemNamespace == "PathOfTerraria.Content.Items.BossDomain" ? new Color(255, 215, 120) : new Color(255, 235, 150);
	}

	internal static float Pulse(int offset)
	{
		return 0.75f + MathF.Sin(Main.GameUpdateCount * 0.055f + offset * 0.37f) * 0.25f;
	}

	internal static void DrawGlow(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, Color color, float rotation, Vector2 origin, float scale)
	{
		for (int i = 0; i < 8; i++)
		{
			Vector2 offset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * 2.5f * scale;
			spriteBatch.Draw(texture, position + offset, frame, color * 0.65f, rotation, origin, scale, SpriteEffects.None, 0f);
		}

		spriteBatch.Draw(texture, position, frame, color * 0.45f, rotation, origin, scale * 1.12f, SpriteEffects.None, 0f);
	}
}

internal sealed class SpecialObjectGlowTile : GlobalTile
{
	private const float TileGlowOpacity = 0.32f;

	public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
	{
		if (!TryGetGlowColor(type, out Color color))
		{
			return;
		}

		Tile tile = Main.tile[i, j];
		Texture2D texture = TextureAssets.Tile[type].Value;
		Rectangle frame = tile.BasicFrame();
		Vector2 position = TileExtensions.DrawPosition(i, j);
		float pulse = SpecialObjectGlowItem.Pulse(i + j * 3);
		Color glowColor = color * (TileGlowOpacity * pulse);

		SpecialObjectGlowItem.DrawGlow(spriteBatch, texture, position + new Vector2(8), frame, glowColor, 0f, new Vector2(8), 1f);
	}

	private static bool TryGetGlowColor(int type, out Color color)
	{
		if (ModContent.GetModTile(type) is ISpecialGlowTile specialGlowTile)
		{
			color = specialGlowTile.GlowColor;
			return true;
		}

		if (type == TileID.Lever && SubworldSystem.Current is BossDomainSubworld)
		{
			color = new Color(255, 220, 120);
			return true;
		}

		color = default;
		return false;
	}
}
