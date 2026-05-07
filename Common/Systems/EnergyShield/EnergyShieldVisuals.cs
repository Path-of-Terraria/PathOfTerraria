using ReLogic.Content;

namespace PathOfTerraria.Common.Systems.EnergyShield;

internal sealed class EnergyShieldVisuals : ModSystem
{
	private const int GlowQuadSize = 96;
	private const int ShatterQuadSize = 144;

	private static readonly Vector3 ShieldColor = new(92f / 255f, 210f / 255f, 255f / 255f);

	private static Asset<Effect> glowEffect;
	private static Asset<Effect> shatterEffect;
	private static Asset<Texture2D> noiseTexture;

	public override void Load()
	{
		if (Main.dedServ)
		{
			return;
		}

		glowEffect = ModContent.Request<Effect>($"{PoTMod.ModName}/Assets/Effects/EnergyShieldGlow");
		shatterEffect = ModContent.Request<Effect>($"{PoTMod.ModName}/Assets/Effects/EnergyShieldShatter");
		noiseTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Misc/ShaderNoiseLooping");

		On_Main.DrawProjectiles += DrawEnergyShields;
	}

	public override void Unload()
	{
		On_Main.DrawProjectiles -= DrawEnergyShields;
		glowEffect = null;
		shatterEffect = null;
		noiseTexture = null;
	}

	private static void DrawEnergyShields(On_Main.orig_DrawProjectiles orig, Main self)
	{
		orig(self);

		if (Main.gameMenu || glowEffect is null || !glowEffect.IsLoaded || !shatterEffect.IsLoaded || !noiseTexture.IsLoaded)
		{
			return;
		}

		Texture2D noise = noiseTexture.Value;
		Matrix transform = Main.GameViewMatrix.TransformationMatrix;
		float time = (float)Main.timeForVisualEffects / 60f;

		bool batchOpen = false;
		Effect activeEffect = null;

		for (int i = 0; i < Main.maxPlayers; i++)
		{
			Player player = Main.player[i];
			if (!player.active || player.dead || player.outOfRange)
			{
				continue;
			}

			EnergyShieldPlayer shield = player.GetModPlayer<EnergyShieldPlayer>();
			bool drawGlow = shield.MaximumEnergyShield > 0 && shield.CurrentEnergyShield > 0;
			bool drawShatter = shield.IsShatterPlaying;

			if (!drawGlow && !drawShatter)
			{
				continue;
			}

			Vector2 center = player.Center - Main.screenPosition;

			if (drawGlow)
			{
				Effect effect = glowEffect.Value;
				if (activeEffect != effect)
				{
					if (batchOpen)
					{
						Main.spriteBatch.End();
					}

					effect.Parameters["uTime"].SetValue(time);
					effect.Parameters["uColor"].SetValue(ShieldColor);
					Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, effect, transform);
					batchOpen = true;
					activeEffect = effect;
				}

				effect.Parameters["uIntensity"].SetValue(shield.ShieldFraction);
				effect.CurrentTechnique.Passes[0].Apply();

				Rectangle dest = new((int)center.X - GlowQuadSize / 2, (int)center.Y - GlowQuadSize / 2, GlowQuadSize, GlowQuadSize);
				Main.spriteBatch.Draw(noise, dest, null, Color.White);
			}

			if (drawShatter)
			{
				Effect effect = shatterEffect.Value;
				if (activeEffect != effect)
				{
					if (batchOpen)
					{
						Main.spriteBatch.End();
					}

					effect.Parameters["uColor"].SetValue(ShieldColor);
					Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, Main.Rasterizer, effect, transform);
					batchOpen = true;
					activeEffect = effect;
				}

				effect.Parameters["uProgress"].SetValue(shield.ShatterProgress);
				effect.CurrentTechnique.Passes[0].Apply();

				Rectangle dest = new((int)center.X - ShatterQuadSize / 2, (int)center.Y - ShatterQuadSize / 2, ShatterQuadSize, ShatterQuadSize);
				Main.spriteBatch.Draw(noise, dest, null, Color.White);
			}
		}

		if (batchOpen)
		{
			Main.spriteBatch.End();
		}
	}
}
