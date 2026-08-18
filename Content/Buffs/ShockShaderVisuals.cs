using PathOfTerraria.Common.Config;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Buffs;

/// <summary>
/// Experimental alternate renderer for shocked entities. Gameplay state remains in
/// <see cref="ShockDebuff"/>; this system only replaces the primitive-based overlay.
/// </summary>
internal sealed class ShockShaderVisuals : ModSystem
{
	/// <summary>
	/// The quad is sized as a multiple of the body rather than a fixed inset, so the rim and halo have room to fall off at any hitbox size.
	/// A flat padding left large NPCs with a quad barely wider than their body, which clipped the effect.
	/// </summary>
	private const float EffectPaddingScale = 1.7f;

	/// <summary>Extra pixels on top of the proportional padding, so tiny hitboxes still get a usable margin.</summary>
	private const float MinimumEffectPadding = 16f;

	private static readonly Vector3 ShockColor = new(0.36f, 0.72f, 1f);

	private static Asset<Effect> shockEffect;

	public override void Load()
	{
		if (Main.dedServ)
		{
			return;
		}

		shockEffect = ModContent.Request<Effect>($"{PoTMod.ModName}/Assets/Effects/ShockedEffect");
		On_Main.DrawProjectiles += DrawShockedEntities;
	}

	public override void Unload()
	{
		On_Main.DrawProjectiles -= DrawShockedEntities;
		shockEffect = null;
	}

	private static void DrawShockedEntities(On_Main.orig_DrawProjectiles orig, Main self)
	{
		orig(self);

		if (Main.gameMenu || !DeveloperConfig.ShaderShockVisualsEnabled
			|| shockEffect is null || !shockEffect.IsLoaded)
		{
			return;
		}

		Effect effect = shockEffect.Value;
		effect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects / 60f);
		effect.Parameters["uColor"].SetValue(ShockColor);

		Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
			DepthStencilState.None, Main.Rasterizer, effect, Main.GameViewMatrix.TransformationMatrix);

		for (int i = 0; i < Main.maxNPCs; i++)
		{
			NPC npc = Main.npc[i];
			// Target dummies use a hidden NPC as their hittable backing entity. Keep
			// skipping other hidden NPCs, but draw the shader around the dummy hitbox.
			if (!npc.active || (npc.hide && npc.type != NPCID.TargetDummy))
			{
				continue;
			}

			ShockedNPC shocked = npc.GetGlobalNPC<ShockedNPC>();
			if (shocked.VisualAlpha <= 0.01f)
			{
				continue;
			}

			Vector2 size = new Vector2(npc.width, npc.height) * npc.scale;
			DrawEffect(effect, npc.Center - Main.screenPosition + new Vector2(0, npc.gfxOffY), size,
				shocked.VisualStrength, shocked.VisualAlpha, npc.whoAmI);
		}

		for (int i = 0; i < Main.maxPlayers; i++)
		{
			Player player = Main.player[i];
			if (!player.active || player.dead || player.outOfRange)
			{
				continue;
			}

			ShockPlayer shocked = player.GetModPlayer<ShockPlayer>();
			if (shocked.VisualAlpha <= 0.01f)
			{
				continue;
			}

			DrawEffect(effect, player.Center - Main.screenPosition, new Vector2(player.width, player.height),
				shocked.VisualStrength, shocked.VisualAlpha, player.whoAmI + Main.maxNPCs);
		}

		Main.spriteBatch.End();
	}

	private static void DrawEffect(Effect effect, Vector2 center, Vector2 bodySize, float strength, float opacity, int seed)
	{
		Vector2 quadSize = bodySize * EffectPaddingScale + new Vector2(MinimumEffectPadding);
		if (!IntersectsScreen(center, quadSize))
		{
			return;
		}

		float power = MathHelper.Clamp(strength / 0.5f, 0.2f, 1f);
		effect.Parameters["uBodyScale"].SetValue(bodySize / quadSize);
		effect.Parameters["uIntensity"].SetValue(opacity * MathHelper.Lerp(0.5f, 0.8f, power));
		effect.Parameters["uSeed"].SetValue(seed * 0.173f);
		effect.CurrentTechnique.Passes[0].Apply();

		Rectangle destination = new(
			(int)(center.X - quadSize.X / 2f),
			(int)(center.Y - quadSize.Y / 2f),
			(int)quadSize.X,
			(int)quadSize.Y);

		// No source rectangle: the shader works off TEXCOORD0 spanning 0-1 across the quad, which a framed draw would only
		// guarantee if MagicPixel is exactly 1x1. Passing null makes that independent of the texture's real size.
		Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, destination, null, Color.White);
	}

	private static bool IntersectsScreen(Vector2 center, Vector2 size)
	{
		return center.X + size.X / 2f >= 0f && center.X - size.X / 2f <= Main.screenWidth
			&& center.Y + size.Y / 2f >= 0f && center.Y - size.Y / 2f <= Main.screenHeight;
	}
}