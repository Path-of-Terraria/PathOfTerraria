using System.Collections.Generic;
using PathOfTerraria.Common.Config;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Utilities;

namespace PathOfTerraria.Content.Buffs;

public sealed class ShockDebuff : ModBuff
{
	private const float MinimumShockEffect = 0.05f;

	/// <summary>
	/// Applies this buff to a given entity (Player or NPC). If victim is an <see cref="NPC"/>, attacker is a <see cref="Player"/>. NPCs cannot apply this buff to other NPCs at this time.
	/// </summary>
	public static void Apply(Entity attacker, Entity victim, int damage)
	{
		if (victim is NPC npc)
		{
			float ailmentThreshold = npc.lifeMax;
			float modifier = attacker is Player player ? player.GetModPlayer<AffixPlayer>().StrengthOf<BuffShockedEffectAffix>() * 0.01f : 0;
			float effect = 0.5f * MathF.Pow(damage / ailmentThreshold, 0.4f) * (1 + modifier);

			if (effect <= MinimumShockEffect)
			{
				if (npc.type != NPCID.TargetDummy)
				{
					return;
				}

				// Target dummies have 1,000 maximum life, which causes weak weapons to
				// fail the ailment threshold even when their chance-to-shock roll succeeds.
				// Give successful dummy procs the minimum effect so they remain useful for
				// testing chance without changing shock balance against real enemies.
				effect = MinimumShockEffect;
			}

			Common.Buffs.DoTFunctionality.ApplyPlayerInteraction(npc, attacker);

			npc.GetGlobalNPC<ShockedNPC>().ShockStrength = effect;
			npc.AddBuff(ModContent.BuffType<ShockDebuff>(), 2 * 60);
		}
		else if (victim is Player player)
		{
			float ailmentThreshold = player.statLifeMax2;
			float modifier = attacker is Player other ? other.GetModPlayer<AffixPlayer>().StrengthOf<BuffShockedEffectAffix>() * 0.01f : 0;
			float effect = 0.5f * MathF.Pow(damage / ailmentThreshold, 0.4f) * (1 + modifier);

			if (effect <= MinimumShockEffect)
			{
				return;
			}

			player.GetModPlayer<ShockPlayer>().ShockEffectiveness = effect;
			player.AddBuff(ModContent.BuffType<ShockDebuff>(), 4 * 60);
		}
	}

	public override void SetStaticDefaults()
	{
		Main.debuff[Type] = true;
		BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
	}

	public override void Update(NPC npc, ref int buffIndex)
	{
		ShockVisuals.AmbientEffects(npc.Center, npc.GetGlobalNPC<ShockedNPC>().ShockStrength);
	}

	public override void Update(Player player, ref int buffIndex)
	{
		ShockVisuals.AmbientEffects(player.Center, player.GetModPlayer<ShockPlayer>().ShockEffectiveness);
	}
}

/// <summary>
/// Draws the forked bolts of electricity that crawl across a shocked entity.<br/>
/// A faint glow sits under them so they read as giving off light.<br/>
/// They are generated procedurally from a seed derived from the entity and the current tick, so no state has to be kept or synced -
/// every client rolls the same bolts and re-rolls them every <see cref="ArcLifetime"/> ticks.
/// </summary>
internal static class ShockVisuals
{
	/// <summary>How many ticks a single set of bolts lives for before a new set is rolled.</summary>
	private const int ArcLifetime = 6;

	/// <summary>Points per bolt, including both endpoints.</summary>
	private const int PointsPerArc = 6;

	/// <summary>
	/// Peak alpha of the glow sitting under the bolts. Deliberately tiny - it should read as the bolts casting a little light,
	/// not as an aura around the entity. This is the single number to change if it wants to be more or less visible.
	/// </summary>
	private const float GlowAlpha = 0.05f;

	private static readonly List<DrawData> Draws = [];
	private static readonly Vector2[] Points = new Vector2[PointsPerArc];

	/// <summary>Explicit single-pixel frame, so segment scale maps straight to pixels regardless of the texture's real size.</summary>
	private static readonly Rectangle PixelFrame = new(0, 0, 1, 1);

	private static Color LightningColor => ItemTooltips.Colors.LightningDamage;

	/// <summary>
	/// Builds every sprite that makes up the effect for an entity. The returned list is reused between calls, so draw it before calling this again.
	/// </summary>
	/// <param name="center">Center of the entity, in screen space.</param>
	/// <param name="size">Size of the entity's hitbox.</param>
	/// <param name="strength">The entity's shock strength, used to scale how busy the effect is.</param>
	/// <param name="opacity">Fade multiplier, used to ease the effect in and out.</param>
	/// <param name="seed">Per-entity seed, so two shocked entities don't arc identically.</param>
	public static List<DrawData> Build(Vector2 center, Vector2 size, float strength, float opacity, int seed)
	{
		Draws.Clear();

		if (opacity <= 0.01f)
		{
			return Draws;
		}

		float power = MathHelper.Clamp(strength / 0.5f, 0.2f, 1f);

		// Bolts snap in bright and decay over their lifetime, which reads as a flicker. The glow shares the
		// same value so it breathes with them instead of sitting there as a constant halo.
		float frameProgress = Main.GameUpdateCount % ArcLifetime / (float)ArcLifetime;
		float fade = opacity * MathHelper.Lerp(1f, 0.3f, frameProgress);

		// Glow first so the bolts draw over the top of it.
		AddGlow(center, size, power, fade);
		AddBolts(center, size, power, seed, fade);

		return Draws;
	}

	/// <summary>
	/// A soft, very faint circle under the bolts, so they look like they are giving off light rather than being flat lines.
	/// Kept inside the hitbox so it never reads as a halo around the entity's silhouette.
	/// </summary>
	private static void AddGlow(Vector2 center, Vector2 size, float power, float fade)
	{
		Texture2D circle = TextureAssets.GlowMask[239].Value;
		float diameter = Math.Max(size.X, size.Y) * 0.9f;
		float alpha = fade * power * GlowAlpha;

		Draws.Add(new DrawData(circle, center, null, LightningColor with { A = 0 } * alpha, 0f, circle.Size() / 2f,
			new Vector2(diameter / circle.Width), SpriteEffects.None, 0));
	}

	/// <summary>
	/// The bolts crawling over the entity, re-rolled every <see cref="ArcLifetime"/> ticks.
	/// </summary>
	private static void AddBolts(Vector2 center, Vector2 size, float power, int seed, float fade)
	{
		int frame = (int)(Main.GameUpdateCount / ArcLifetime);

		var rand = new UnifiedRandom(HashCode.Combine(seed, frame));
		int boltCount = 1 + (int)MathF.Round(power * 2);
		Vector2 radius = size / 2f;

		for (int i = 0; i < boltCount; ++i)
		{
			float startAngle = rand.NextFloat(MathHelper.TwoPi);
			float endAngle = startAngle + MathHelper.Pi * rand.NextFloat(0.4f, 1.6f);

			Vector2 start = center + startAngle.ToRotationVector2() * radius * rand.NextFloat(0.5f, 0.95f);
			Vector2 end = center + endAngle.ToRotationVector2() * radius * rand.NextFloat(0.5f, 0.95f);

			// Each bolt gets its own brightness so the cluster looks alive rather than pulsing as one.
			float brightness = fade * rand.NextFloat(0.65f, 1f);

			AddBolt(start, end, brightness, 1f, rand);

			// Forks are what make a bolt read as lightning instead of a bent line.
			if (rand.NextBool(2))
			{
				Vector2 from = Vector2.Lerp(start, end, rand.NextFloat(0.3f, 0.7f));
				Vector2 to = from + (end - start).RotatedBy(rand.NextFloat(-1.1f, 1.1f)) * rand.NextFloat(0.25f, 0.5f);

				AddBolt(from, to, brightness * 0.7f, 0.55f, rand);
			}
		}
	}

	/// <summary>
	/// Adds one jagged bolt as a stack of three passes - a wide dim bloom, a mid glow and a thin white core - tapered towards both ends.
	/// </summary>
	private static void AddBolt(Vector2 from, Vector2 to, float brightness, float thickness, UnifiedRandom rand)
	{
		Texture2D pixel = TextureAssets.MagicPixel.Value;

		Vector2 span = to - from;
		Vector2 jitterDirection = span.SafeNormalize(Vector2.UnitY).RotatedBy(MathHelper.PiOver2);
		float jitter = span.Length() * 0.16f;

		Points[0] = from;
		Points[PointsPerArc - 1] = to;

		for (int i = 1; i < PointsPerArc - 1; ++i)
		{
			float along = i / (float)(PointsPerArc - 1);
			Points[i] = from + span * along + jitterDirection * rand.NextFloat(-jitter, jitter);
		}

		Color bloom = LightningColor with { A = 0 } * (brightness * 0.16f);
		Color glow = LightningColor with { A = 0 } * (brightness * 0.55f);
		Color core = Color.White with { A = 0 } * brightness;
		var origin = new Vector2(0f, 0.5f);

		for (int i = 0; i < PointsPerArc - 1; ++i)
		{
			Vector2 segment = Points[i + 1] - Points[i];
			float rotation = segment.ToRotation();
			float length = segment.Length();

			// Taper towards both ends so the bolt has a body instead of reading as a uniform stroke.
			float taper = MathF.Sin((i + 0.5f) / (PointsPerArc - 1) * MathHelper.Pi) * thickness;

			Draws.Add(new DrawData(pixel, Points[i], PixelFrame, bloom, rotation, origin, new Vector2(length, 9f * taper + 2f), SpriteEffects.None, 0));
			Draws.Add(new DrawData(pixel, Points[i], PixelFrame, glow, rotation, origin, new Vector2(length, 4f * taper + 1f), SpriteEffects.None, 0));
			Draws.Add(new DrawData(pixel, Points[i], PixelFrame, core, rotation, origin, new Vector2(length, 1.6f * taper + 0.6f), SpriteEffects.None, 0));
		}
	}

	/// <summary>
	/// Non-drawing effects that go along with the bolts - a flickering light so shocked entities are visible in the dark.
	/// </summary>
	public static void AmbientEffects(Vector2 center, float strength)
	{
		if (Main.dedServ)
		{
			return;
		}

		float power = MathHelper.Clamp(strength / 0.5f, 0.2f, 1f);
		float flicker = 0.3f + Main.rand.NextFloat(0.2f);

		Lighting.AddLight(center, LightningColor.ToVector3() * power * flicker);
	}
}

public sealed class ShockedNPC : GlobalNPC
{
	public override bool InstancePerEntity => true;

	private float shockStrength = 0;

	/// <summary>Last non-zero shock strength, kept so the visuals can fade out after the buff drops.</summary>
	private float visualStrength = 0;

	private float visualAlpha = 0;

	internal float VisualStrength => visualStrength;

	internal float VisualAlpha => visualAlpha;

	public float ShockStrength
	{
		get => shockStrength;
		set => shockStrength = MathHelper.Clamp(value, 0, 0.5f);
	}

	public override void ResetEffects(NPC npc)
	{
		bool shocked = npc.HasBuff<ShockDebuff>();

		if (!shocked)
		{
			shockStrength = 0;
		}
		else
		{
			visualStrength = shockStrength;
		}

		visualAlpha = MathHelper.Lerp(visualAlpha, shocked ? 1f : 0f, 0.15f);
	}

	public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
	{
		if (npc.HasBuff<ShockDebuff>())
		{
			modifiers.FinalDamage += ShockStrength;
		}
	}

	public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
	{
		if (npc.HasBuff<ShockDebuff>())
		{
			modifiers.FinalDamage += ShockStrength;
		}
	}

	public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
	{
		if (DeveloperConfig.ShaderShockVisualsEnabled || visualAlpha <= 0.01f)
		{
			return;
		}

		Vector2 center = npc.Center - screenPos + new Vector2(0, npc.gfxOffY);
		Vector2 size = new Vector2(npc.width, npc.height) * npc.scale;

		foreach (DrawData data in ShockVisuals.Build(center, size, visualStrength, visualAlpha, npc.whoAmI))
		{
			data.Draw(spriteBatch);
		}
	}
}

public class ShockPlayer : ModPlayer
{
	public float ShockEffectiveness { get; set; }

	/// <summary>Last non-zero shock strength, kept so the visuals can fade out after the buff drops.</summary>
	internal float VisualStrength;

	internal float VisualAlpha;

	public override void ResetEffects()
	{
		bool shocked = Player.HasBuff<ShockDebuff>();

		if (shocked)
		{
			VisualStrength = ShockEffectiveness;
		}

		VisualAlpha = MathHelper.Lerp(VisualAlpha, shocked ? 1f : 0f, 0.15f);
	}

	public override void ModifyHurt(ref Player.HurtModifiers modifiers)
	{
		if (Player.HasBuff<ShockDebuff>())
		{
			modifiers.FinalDamage += ShockEffectiveness;
		}
	}
}

internal sealed class ShockPlayerLayer : PlayerDrawLayer
{
	public override Position GetDefaultPosition()
	{
		return new AfterParent(PlayerDrawLayers.HeldItem);
	}

	protected override void Draw(ref PlayerDrawSet drawInfo)
	{
		Player player = drawInfo.drawPlayer;
		ShockPlayer shock = player.GetModPlayer<ShockPlayer>();

		if (DeveloperConfig.ShaderShockVisualsEnabled || player.dead || shock.VisualAlpha <= 0.01f)
		{
			return;
		}

		Vector2 center = drawInfo.Center - Main.screenPosition;
		Vector2 size = new(player.width, player.height);

		foreach (DrawData data in ShockVisuals.Build(center, size, shock.VisualStrength, shock.VisualAlpha, player.whoAmI))
		{
			drawInfo.DrawDataCache.Add(data);
		}
	}
}