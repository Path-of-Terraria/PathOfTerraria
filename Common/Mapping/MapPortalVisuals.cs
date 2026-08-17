using PathOfTerraria.Content.Items.Consumables.Maps;
using PathOfTerraria.Content.Items.Consumables.Maps.BossMaps;
using PathOfTerraria.Content.Items.Consumables.Maps.ExplorableMaps;
using Terraria.GameContent;

#nullable enable

namespace PathOfTerraria.Common.Mapping;

/// <summary>
/// A code-driven portal effect. Destination-specific profiles keep the look easy to tune without
/// baking animation, color, or timing into a sprite sheet.
/// </summary>
internal static class MapPortalVisuals
{
	private const float SpinSpeed = 7f;

	private enum Motif
	{
		Compass,
		Slime,
		Eye,
		Worm,
		Brain,
		Hive,
		Frost,
		Skull,
		Flame,
		Crystal,
		Twins,
		Destroyer,
		Prime,
		Petals,
		Temple,
		Tide,
		Prism,
		Runes,
		Lunar,
	}

	private readonly record struct Profile(
		Color Primary,
		Color Secondary,
		Color Accent,
		Motif Motif,
		int OrbitCount,
		float IconScale = 1f,
		float RingScale = 1f,
		float Phase = 0f);

	public static void Draw(
		SpriteBatch spriteBatch,
		Texture2D swirlTexture,
		Texture2D glowTexture,
		Texture2D starTexture,
		Vector2 position,
		float openingAnimation,
		Color lightColor,
		Item storedMap,
		int? injectedItemType,
		Color fallbackColor)
	{
		Map? map = storedMap?.ModItem as Map;
		Profile profile = GetProfile(map, fallbackColor);
		float time = Main.GlobalTimeWrappedHourly;
		float opening = MathHelper.SmoothStep(0f, 1f, openingAnimation);
		float pulse = 1f + MathF.Sin(time * MathHelper.TwoPi / 2f + profile.Phase) * 0.045f;
		float spin = time * SpinSpeed + profile.Phase;
		float radius = 47f * opening * profile.RingScale * pulse;

		if (radius <= 0.1f)
		{
			return;
		}

		DrawGlow(spriteBatch, glowTexture, position, profile.Secondary, radius * 2.9f, 0.22f * opening);
		DrawGlow(spriteBatch, glowTexture, position, profile.Primary, radius * 2.15f, 0.30f * opening);

		// The old portal rotated at seven radians per second. Every animated layer now follows that
		// same clockwise direction, with slight speed offsets to keep the movement fluid.
		Vector2 swirlOrigin = swirlTexture.Size() * 0.5f;
		float swirlScale = radius * 2f / swirlTexture.Width;
		for (int layer = 0; layer < 4; layer++)
		{
			float layerScale = swirlScale * (1f - layer * 0.115f);
			float layerRotation = spin * (1f - layer * 0.055f) + layer * 1.37f;
			Color layerColor = Color.Lerp(profile.Primary, profile.Secondary, layer / 3f);
			DrawAdditive(spriteBatch, swirlTexture, position, null, layerColor, 0.34f - layer * 0.045f,
				layerRotation, swirlOrigin, layerScale, SpriteEffects.None);
		}

		DrawEnergyRing(spriteBatch, position, radius, spin, time, profile, opening);
		DrawMotif(spriteBatch, glowTexture, starTexture, position, radius, spin, time, profile, opening);
		DrawOrbit(spriteBatch, glowTexture, starTexture, position, radius, spin, time, profile, opening);

		int iconItemType = storedMap is { IsAir: false } ? storedMap.type : injectedItemType ?? 0;
		if (iconItemType > 0)
		{
			DrawDestinationIcon(spriteBatch, glowTexture, position, iconItemType, profile, lightColor, opening);
		}
	}

	private static Profile GetProfile(Map? map, Color fallbackColor)
	{
		return map switch
		{
			KingSlimeMap => new(new(45, 154, 255), new(58, 232, 255), Color.White, Motif.Slime, 7, 1.05f, 1f, 0.1f),
			EoCMap => new(new(218, 42, 63), new(255, 119, 82), new(255, 226, 181), Motif.Eye, 5, 1.08f, 1f, 0.7f),
			EoWMap => new(new(111, 47, 190), new(210, 67, 255), new(116, 255, 179), Motif.Worm, 11, 1.04f, 1.05f, 1.4f),
			BoCMap => new(new(190, 34, 86), new(255, 75, 167), new(131, 240, 255), Motif.Brain, 6, 1.05f, 1.03f, 2.1f),
			BeeMap => new(new(255, 169, 24), new(255, 225, 88), new(92, 52, 24), Motif.Hive, 6, 1.02f, 1f, 2.8f),
			DeerclopsMap => new(new(65, 142, 206), new(172, 239, 255), Color.White, Motif.Frost, 8, 1.06f, 1.06f, 3.5f),
			SkeletronMap => new(new(112, 116, 145), new(229, 220, 190), new(116, 191, 255), Motif.Skull, 6, 1.05f, 1.03f, 4.2f),
			WoFMap => new(new(217, 43, 20), new(255, 133, 26), new(255, 229, 102), Motif.Flame, 9, 1.04f, 1.08f, 4.9f),
			QueenSlimeMap => new(new(185, 82, 255), new(255, 122, 226), new(111, 242, 255), Motif.Crystal, 8, 1.05f, 1.04f, 5.6f),
			TwinsMap => new(new(245, 56, 67), new(71, 232, 146), new(188, 255, 242), Motif.Twins, 6, 1.08f, 1.05f, 0.45f),
			DestroyerMap => new(new(108, 68, 194), new(74, 211, 255), new(223, 229, 255), Motif.Destroyer, 12, 1.04f, 1.08f, 1.15f),
			PrimeMap => new(new(126, 139, 164), new(255, 101, 45), new(235, 245, 255), Motif.Prime, 8, 1.06f, 1.06f, 1.85f),
			PlanteraMap => new(new(231, 54, 131), new(103, 211, 82), new(255, 209, 121), Motif.Petals, 8, 1.06f, 1.08f, 2.55f),
			GolemMap => new(new(188, 75, 24), new(255, 177, 53), new(255, 231, 134), Motif.Temple, 8, 1.05f, 1.06f, 3.25f),
			FishronMap => new(new(28, 164, 186), new(69, 229, 207), new(184, 246, 255), Motif.Tide, 9, 1.08f, 1.09f, 3.95f),
			EoLMap => new(new(255, 126, 199), new(255, 221, 104), new(116, 235, 255), Motif.Prism, 12, 1.05f, 1.09f, 4.65f),
			CultistMap => new(new(47, 108, 222), new(145, 75, 230), new(93, 240, 255), Motif.Runes, 7, 1.06f, 1.08f, 5.35f),
			MoonMap => new(new(32, 154, 158), new(90, 232, 221), new(185, 142, 255), Motif.Lunar, 10, 1.08f, 1.12f, 6.05f),

			// Explorable maps intentionally share the same compass construction. Their palette is the
			// only destination cue, so they read as one polished visual family.
			ForestMap => new(new(49, 155, 91), new(128, 220, 109), new(222, 247, 174), Motif.Compass, 8, 1.05f, 1f, 0.3f),
			DesertMap => new(new(203, 135, 43), new(250, 207, 92), new(255, 241, 185), Motif.Compass, 8, 1.05f, 1f, 1.7f),
			SwampMap => new(new(99, 64, 157), new(169, 101, 198), new(174, 232, 146), Motif.Compass, 8, 1.05f, 1f, 3.1f),
			ExplorableMap => new(new(0, 151, 255), new(92, 220, 255), Color.White, Motif.Compass, 8),
			_ => new(fallbackColor, Color.Lerp(fallbackColor, Color.White, 0.38f), Color.White, Motif.Compass, 8),
		};
	}

	private static void DrawEnergyRing(SpriteBatch spriteBatch, Vector2 center, float radius, float spin, float time,
		Profile profile, float opacity)
	{
		const int segments = 72;
		for (int i = 0; i < segments; i++)
		{
			float progress = i / (float)segments;
			float nextProgress = (i + 1) / (float)segments;
			float wave = MathF.Sin(progress * MathHelper.TwoPi * 6f - time * SpinSpeed * 0.55f + profile.Phase);
			float nextWave = MathF.Sin(nextProgress * MathHelper.TwoPi * 6f - time * SpinSpeed * 0.55f + profile.Phase);
			float alpha = 0.24f + (wave * 0.5f + 0.5f) * 0.48f;
			float angle = spin + progress * MathHelper.TwoPi;
			float nextAngle = spin + nextProgress * MathHelper.TwoPi;
			Vector2 start = center + angle.ToRotationVector2() * (radius + wave * 1.6f);
			Vector2 end = center + nextAngle.ToRotationVector2() * (radius + nextWave * 1.6f);
			Color color = Color.Lerp(profile.Primary, profile.Secondary, progress);
			DrawLine(spriteBatch, start, end, color, (1.25f + alpha) * opacity, alpha * opacity);
		}
	}

	private static void DrawOrbit(SpriteBatch spriteBatch, Texture2D glowTexture, Texture2D starTexture, Vector2 center,
		float radius, float spin, float time, Profile profile, float opacity)
	{
		for (int i = 0; i < profile.OrbitCount; i++)
		{
			float progress = i / (float)profile.OrbitCount;
			float angle = spin * 0.92f + progress * MathHelper.TwoPi;
			float orbitRadius = radius * (1.13f + MathF.Sin(time * 2.2f + i * 2.13f) * 0.055f);
			Vector2 point = center + angle.ToRotationVector2() * orbitRadius;
			Color color = Color.Lerp(profile.Primary, profile.Accent, MathF.Sin(time * 2f + i) * 0.5f + 0.5f);
			DrawGlow(spriteBatch, glowTexture, point, color, 13f, 0.25f * opacity);
			DrawAdditive(spriteBatch, starTexture, point, null, color, 0.68f * opacity, angle,
				starTexture.Size() * 0.5f, 0.10f + (i % 3) * 0.018f, SpriteEffects.None);
		}
	}

	private static void DrawMotif(SpriteBatch spriteBatch, Texture2D glowTexture, Texture2D starTexture, Vector2 center,
		float radius, float spin, float time, Profile profile, float opacity)
	{
		switch (profile.Motif)
		{
			case Motif.Slime:
				DrawBubbles(spriteBatch, glowTexture, center, radius, spin, profile, opacity, 7, 0.62f, 0.96f);
				break;
			case Motif.Eye:
				DrawSpokes(spriteBatch, center, radius, spin, profile, opacity, 6, 0.56f, 0.88f);
				DrawPolygon(spriteBatch, center, radius * 0.73f, spin, profile.Accent, opacity * 0.45f, 12);
				break;
			case Motif.Worm:
				DrawSpiral(spriteBatch, glowTexture, center, radius, spin, profile, opacity, 13);
				break;
			case Motif.Brain:
				DrawBubbles(spriteBatch, glowTexture, center + spin.ToRotationVector2() * 5f, radius, spin, profile, opacity, 5, 0.58f, 0.82f);
				DrawBubbles(spriteBatch, glowTexture, center - spin.ToRotationVector2() * 5f, radius, spin + MathF.PI, profile, opacity, 5, 0.58f, 0.82f);
				break;
			case Motif.Hive:
				DrawPolygon(spriteBatch, center, radius * 0.82f, spin, profile.Accent, opacity * 0.65f, 6);
				DrawPolygon(spriteBatch, center, radius * 0.61f, spin + MathF.PI / 6f, profile.Secondary, opacity * 0.45f, 6);
				break;
			case Motif.Frost:
				DrawSpokes(spriteBatch, center, radius, spin, profile, opacity, 6, 0.42f, 0.93f);
				DrawSpokes(spriteBatch, center, radius, spin + MathF.PI / 6f, profile, opacity * 0.55f, 6, 0.62f, 0.84f);
				break;
			case Motif.Skull:
				DrawSpokes(spriteBatch, center, radius, spin, profile, opacity, 4, 0.54f, 0.93f);
				DrawPolygon(spriteBatch, center, radius * 0.76f, spin + MathF.PI / 4f, profile.Secondary, opacity * 0.52f, 4);
				break;
			case Motif.Flame:
				DrawSparks(spriteBatch, starTexture, center, radius, spin, profile, opacity, 9, 0.68f, 0.96f, 0.16f);
				break;
			case Motif.Crystal:
				DrawPolygon(spriteBatch, center, radius * 0.89f, spin, profile.Accent, opacity * 0.72f, 4);
				DrawSparks(spriteBatch, starTexture, center, radius, spin + MathF.PI / 4f, profile, opacity, 4, 0.61f, 0.83f, 0.13f);
				break;
			case Motif.Twins:
				DrawTwinCores(spriteBatch, glowTexture, starTexture, center, radius, spin, profile, opacity);
				break;
			case Motif.Destroyer:
				DrawSpiral(spriteBatch, glowTexture, center, radius, spin, profile, opacity, 16);
				DrawPolygon(spriteBatch, center, radius * 0.72f, spin, profile.Accent, opacity * 0.35f, 8);
				break;
			case Motif.Prime:
				DrawGear(spriteBatch, center, radius, spin, profile, opacity, 8);
				break;
			case Motif.Petals:
				DrawBubbles(spriteBatch, glowTexture, center, radius, spin, profile, opacity, 8, 0.55f, 0.86f);
				DrawPolygon(spriteBatch, center, radius * 0.68f, spin + MathF.PI / 8f, profile.Accent, opacity * 0.42f, 8);
				break;
			case Motif.Temple:
				DrawPolygon(spriteBatch, center, radius * 0.88f, spin, profile.Secondary, opacity * 0.7f, 4);
				DrawPolygon(spriteBatch, center, radius * 0.65f, spin + MathF.PI / 4f, profile.Accent, opacity * 0.48f, 4);
				break;
			case Motif.Tide:
				DrawBubbles(spriteBatch, glowTexture, center, radius, spin, profile, opacity, 9, 0.54f, 0.96f);
				DrawPolygon(spriteBatch, center, radius * 0.73f, spin, profile.Accent, opacity * 0.35f, 12);
				break;
			case Motif.Prism:
				DrawSparks(spriteBatch, starTexture, center, radius, spin, profile, opacity, 12, 0.59f, 0.94f, 0.14f);
				DrawSpokes(spriteBatch, center, radius, spin, profile, opacity * 0.45f, 12, 0.62f, 0.85f);
				break;
			case Motif.Runes:
				DrawPolygon(spriteBatch, center, radius * 0.88f, spin, profile.Accent, opacity * 0.64f, 3);
				DrawPolygon(spriteBatch, center, radius * 0.68f, spin + MathF.PI, profile.Secondary, opacity * 0.5f, 3);
				break;
			case Motif.Lunar:
				DrawSparks(spriteBatch, starTexture, center, radius, spin, profile, opacity, 6, 0.55f, 0.92f, 0.16f);
				DrawPolygon(spriteBatch, center, radius * 0.76f, spin + time * 0.35f, profile.Accent, opacity * 0.48f, 6);
				break;
			default:
				DrawSpokes(spriteBatch, center, radius, spin, profile, opacity, 4, 0.5f, 0.92f);
				DrawSpokes(spriteBatch, center, radius, spin + MathF.PI / 4f, profile, opacity * 0.45f, 4, 0.68f, 0.83f);
				break;
		}
	}

	private static void DrawBubbles(SpriteBatch spriteBatch, Texture2D glowTexture, Vector2 center, float radius,
		float rotation, Profile profile, float opacity, int count, float innerRadius, float outerRadius)
	{
		for (int i = 0; i < count; i++)
		{
			float progress = i / (float)count;
			float angle = rotation + progress * MathHelper.TwoPi;
			float distance = radius * MathHelper.Lerp(innerRadius, outerRadius, i % 2);
			Vector2 point = center + angle.ToRotationVector2() * distance;
			Color color = Color.Lerp(profile.Primary, profile.Secondary, progress);
			DrawGlow(spriteBatch, glowTexture, point, color, 12f + i % 3 * 3f, opacity * 0.34f);
		}
	}

	private static void DrawSpiral(SpriteBatch spriteBatch, Texture2D glowTexture, Vector2 center, float radius,
		float rotation, Profile profile, float opacity, int count)
	{
		for (int i = 0; i < count; i++)
		{
			float progress = i / (float)count;
			float angle = rotation + progress * MathHelper.TwoPi;
			float distance = radius * MathHelper.Lerp(0.58f, 0.98f, progress);
			Vector2 point = center + angle.ToRotationVector2() * distance;
			Color color = Color.Lerp(profile.Primary, profile.Accent, progress);
			DrawGlow(spriteBatch, glowTexture, point, color, 8f + progress * 7f, opacity * MathHelper.Lerp(0.18f, 0.5f, progress));
		}
	}

	private static void DrawSparks(SpriteBatch spriteBatch, Texture2D starTexture, Vector2 center, float radius,
		float rotation, Profile profile, float opacity, int count, float innerRadius, float outerRadius, float scale)
	{
		for (int i = 0; i < count; i++)
		{
			float progress = i / (float)count;
			float angle = rotation + progress * MathHelper.TwoPi;
			float distance = radius * MathHelper.Lerp(innerRadius, outerRadius, i % 2);
			Color color = Color.Lerp(profile.Primary, i % 3 == 0 ? profile.Accent : profile.Secondary, progress);
			Vector2 point = center + angle.ToRotationVector2() * distance;
			DrawAdditive(spriteBatch, starTexture, point, null, color, opacity * 0.63f, angle,
				starTexture.Size() * 0.5f, scale * (0.8f + i % 3 * 0.12f), SpriteEffects.None);
		}
	}

	private static void DrawTwinCores(SpriteBatch spriteBatch, Texture2D glowTexture, Texture2D starTexture,
		Vector2 center, float radius, float rotation, Profile profile, float opacity)
	{
		for (int i = 0; i < 2; i++)
		{
			float angle = rotation + i * MathF.PI;
			Vector2 point = center + angle.ToRotationVector2() * radius * 0.76f;
			Color color = i == 0 ? profile.Primary : profile.Secondary;
			DrawGlow(spriteBatch, glowTexture, point, color, 27f, opacity * 0.55f);
			DrawAdditive(spriteBatch, starTexture, point, null, color, opacity * 0.82f, angle,
				starTexture.Size() * 0.5f, 0.22f, SpriteEffects.None);
			DrawLine(spriteBatch, center, point, color, 1.5f * opacity, 0.32f * opacity);
		}
	}

	private static void DrawGear(SpriteBatch spriteBatch, Vector2 center, float radius, float rotation,
		Profile profile, float opacity, int teeth)
	{
		DrawPolygon(spriteBatch, center, radius * 0.76f, rotation + MathF.PI / teeth, profile.Accent, opacity * 0.52f, teeth);
		DrawSpokes(spriteBatch, center, radius, rotation, profile, opacity, teeth, 0.67f, 0.98f);
	}

	private static void DrawSpokes(SpriteBatch spriteBatch, Vector2 center, float radius, float rotation,
		Profile profile, float opacity, int count, float innerRadius, float outerRadius)
	{
		for (int i = 0; i < count; i++)
		{
			float progress = i / (float)count;
			float angle = rotation + progress * MathHelper.TwoPi;
			Vector2 direction = angle.ToRotationVector2();
			Color color = Color.Lerp(profile.Primary, profile.Accent, progress);
			DrawLine(spriteBatch, center + direction * radius * innerRadius, center + direction * radius * outerRadius,
				color, 1.65f * opacity, 0.55f * opacity);
		}
	}

	private static void DrawPolygon(SpriteBatch spriteBatch, Vector2 center, float radius, float rotation,
		Color color, float opacity, int sides)
	{
		for (int i = 0; i < sides; i++)
		{
			float angle = rotation + i / (float)sides * MathHelper.TwoPi;
			float nextAngle = rotation + (i + 1) / (float)sides * MathHelper.TwoPi;
			DrawLine(spriteBatch, center + angle.ToRotationVector2() * radius,
				center + nextAngle.ToRotationVector2() * radius, color, 1.35f * opacity, 0.5f * opacity);
		}
	}

	private static void DrawDestinationIcon(SpriteBatch spriteBatch, Texture2D glowTexture, Vector2 center,
		int itemType, Profile profile, Color lightColor, float opacity)
	{
		Texture2D iconTexture = TextureAssets.Item[itemType].Value;
		float iconScale = 35f / MathF.Max(iconTexture.Width, iconTexture.Height) * profile.IconScale;
		Vector2 iconOrigin = iconTexture.Size() * 0.5f;
		Color iconColor = Color.Lerp(lightColor, Color.White, 0.85f) * opacity;

		DrawGlow(spriteBatch, glowTexture, center, Color.Black, 50f, 0.54f * opacity, additive: false);
		DrawGlow(spriteBatch, glowTexture, center, profile.Primary, 47f, 0.28f * opacity);
		DrawGlow(spriteBatch, glowTexture, center, profile.Accent, 33f, 0.18f * opacity);

		for (int i = 0; i < 4; i++)
		{
			Vector2 shadowOffset = (i * MathHelper.PiOver2).ToRotationVector2() * 1.5f;
			spriteBatch.Draw(iconTexture, center + shadowOffset, null, Color.Black * (0.72f * opacity), 0f,
				iconOrigin, iconScale, SpriteEffects.None, 0f);
		}

		spriteBatch.Draw(iconTexture, center, null, iconColor, 0f, iconOrigin, iconScale, SpriteEffects.None, 0f);
	}

	private static void DrawGlow(SpriteBatch spriteBatch, Texture2D texture, Vector2 center, Color color,
		float diameter, float opacity, bool additive = true)
	{
		Vector2 scale = new(diameter / texture.Width, diameter / texture.Height);
		if (additive)
		{
			DrawAdditive(spriteBatch, texture, center, null, color, opacity, 0f, texture.Size() * 0.5f, scale, SpriteEffects.None);
		}
		else
		{
			spriteBatch.Draw(texture, center, null, color * opacity, 0f, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
		}
	}

	private static void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float width, float opacity)
	{
		Vector2 difference = end - start;
		float length = difference.Length();
		if (length <= 0.01f)
		{
			return;
		}

		Texture2D pixel = TextureAssets.MagicPixel.Value;
		// Tile rendering does not guarantee that MagicPixel is bound as a one-pixel source. Drawing the
		// full texture here can multiply the requested line dimensions and turn small portal segments
		// into screen-wide rays. Pin the source to one texel so length and width remain pixel units.
		Rectangle source = new(0, 0, 1, 1);
		DrawAdditive(spriteBatch, pixel, start, source, color, opacity, difference.ToRotation(), new Vector2(0f, 0.5f),
			new Vector2(length, MathF.Max(0.5f, width)), SpriteEffects.None);
	}

	private static void DrawAdditive(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle? source,
		Color color, float opacity, float rotation, Vector2 origin, float scale, SpriteEffects effects)
	{
		DrawAdditive(spriteBatch, texture, position, source, color, opacity, rotation, origin, new Vector2(scale), effects);
	}

	private static void DrawAdditive(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle? source,
		Color color, float opacity, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects)
	{
		Color additiveColor = color * MathHelper.Clamp(opacity, 0f, 1f);
		additiveColor.A = 0;
		spriteBatch.Draw(texture, position, source, additiveColor, rotation, origin, scale, effects, 0f);
	}
}
