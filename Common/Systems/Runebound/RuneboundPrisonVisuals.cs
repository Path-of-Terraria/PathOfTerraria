using ReLogic.Content;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Runebound;

/// <summary>
/// Draws the animated binding around sealed Runebound enemies and provides family-specific material particles.
/// The overlay is a separate additive pass so arbitrary vanilla NPC animations remain intact.
/// </summary>
internal sealed class RuneboundPrisonVisuals : ModSystem
{
	private static Asset<Effect> prisonEffect;
	private static Asset<Texture2D> noiseTexture;

	public override void Load()
	{
		if (Main.dedServ)
		{
			return;
		}

		prisonEffect = ModContent.Request<Effect>($"{PoTMod.ModName}/Assets/Effects/RuneboundPrison", AssetRequestMode.ImmediateLoad);
		noiseTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Misc/ShaderNoiseLooping", AssetRequestMode.ImmediateLoad);
		On_Main.DoDraw_DrawNPCsOverTiles += DrawPrisons;
	}

	public override void Unload()
	{
		On_Main.DoDraw_DrawNPCsOverTiles -= DrawPrisons;
		prisonEffect = null;
		noiseTexture = null;
	}

	private static void DrawPrisons(On_Main.orig_DoDraw_DrawNPCsOverTiles orig, Main self)
	{
		orig(self);

		if (Main.gameMenu || prisonEffect is null || noiseTexture is null || !prisonEffect.IsLoaded || !noiseTexture.IsLoaded)
		{
			return;
		}

		Effect effect = prisonEffect.Value;
		Matrix transform = Main.GameViewMatrix.TransformationMatrix;
		float time = (float)Main.timeForVisualEffects / 60f;
		bool batchOpen = false;
		Rectangle visibleArea = new((int)Main.screenPosition.X - 180, (int)Main.screenPosition.Y - 180,
			Main.screenWidth + 360, Main.screenHeight + 360);

		for (int i = 0; i < Main.maxNPCs; i++)
		{
			NPC npc = Main.npc[i];
			if (!npc.active || npc.IsABestiaryIconDummy || !visibleArea.Intersects(npc.Hitbox))
			{
				continue;
			}

			RuneboundNPC binding = npc.GetGlobalNPC<RuneboundNPC>();
			if (!binding.Sealed)
			{
				continue;
			}

			if (!batchOpen)
			{
				Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap,
					DepthStencilState.None, Main.Rasterizer, effect, transform);
				batchOpen = true;
			}

			Color primary = RuneboundSystem.GetFamilyColor(binding.Family);
			Color secondary = GetSecondaryColor(binding.Family);
			int width = Math.Max(92, npc.width + 78);
			int height = Math.Max(116, npc.height + 96);
			Vector2 center = npc.Center - Main.screenPosition + new Vector2(0f, npc.gfxOffY);

			effect.Parameters["uTime"].SetValue(time);
			effect.Parameters["uPrimary"].SetValue(primary.ToVector3());
			effect.Parameters["uSecondary"].SetValue(secondary.ToVector3());
			effect.Parameters["uMaterial"].SetValue((float)binding.Family);
			effect.Parameters["uGrade"].SetValue((float)binding.Grade);
			effect.Parameters["uSeed"].SetValue(i * 0.173f % 1f);
			effect.CurrentTechnique.Passes[0].Apply();

			Rectangle destination = new((int)center.X - width / 2, (int)center.Y - height / 2, width, height);
			Main.spriteBatch.Draw(noiseTexture.Value, destination, null, Color.White);
		}

		if (batchOpen)
		{
			Main.spriteBatch.End();
		}
	}

	internal static void UpdateMaterialEffects(NPC npc, RuneboundFamily family, RunestoneGrade grade)
	{
		if (Main.dedServ)
		{
			return;
		}

		Color color = RuneboundSystem.GetFamilyColor(family);
		Lighting.AddLight(npc.Center, color.ToVector3() * (0.45f + (int)grade * 0.12f));

		if (!Main.rand.NextBool(16 - (int)grade * 3))
		{
			return;
		}

		float angle = Main.rand.NextFloat(MathHelper.TwoPi);
		Vector2 radial = angle.ToRotationVector2();
		Vector2 position = npc.Center + radial * new Vector2(npc.width * 0.65f + 18f, npc.height * 0.55f + 24f);
		Vector2 velocity = radial.RotatedBy(MathHelper.PiOver2) * 0.55f;
		int dustType = GetDustType(family);

		switch (family)
		{
			case RuneboundFamily.Vigor:
				velocity = -Vector2.UnitY * Main.rand.NextFloat(0.25f, 0.9f);
				break;
			case RuneboundFamily.Bastion:
				velocity *= 0.15f;
				break;
			case RuneboundFamily.Embers:
				velocity = new Vector2(Main.rand.NextFloat(-0.35f, 0.35f), Main.rand.NextFloat(-1.8f, -0.8f));
				break;
			case RuneboundFamily.Rime:
				velocity = radial * Main.rand.NextFloat(0.25f, 0.8f);
				break;
			case RuneboundFamily.Tempests:
				velocity = radial.RotatedBy(Main.rand.NextFloat(-0.7f, 0.7f)) * Main.rand.NextFloat(1.5f, 3f);
				break;
			case RuneboundFamily.Void:
				velocity = -radial * 0.9f + radial.RotatedBy(MathHelper.PiOver2) * 0.65f;
				break;
			case RuneboundFamily.Might:
				velocity = radial * Main.rand.NextFloat(0.7f, 1.4f);
				break;
			case RuneboundFamily.Precision:
				velocity = -radial * 0.35f;
				break;
			case RuneboundFamily.Haste:
				velocity = new Vector2(Main.rand.NextBool() ? 2.4f : -2.4f, Main.rand.NextFloat(-0.25f, 0.25f));
				break;
			case RuneboundFamily.Spirit:
				velocity = new Vector2(MathF.Sin(angle * 2f) * 0.5f, Main.rand.NextFloat(-1.2f, -0.45f));
				break;
		}

		Dust dust = Dust.NewDustPerfect(position, dustType, velocity, 80, color, Main.rand.NextFloat(0.8f, 1.25f));
		dust.noGravity = true;
		dust.fadeIn = 0.9f;
	}

	internal static void CreateReleaseBurst(NPC npc, RuneboundFamily family, RunestoneGrade grade)
	{
		if (Main.dedServ)
		{
			return;
		}

		Color color = RuneboundSystem.GetFamilyColor(family);
		int dustType = GetDustType(family);
		int burstCount = 32 + (int)grade * 12;
		for (int i = 0; i < burstCount; i++)
		{
			Vector2 direction = (MathHelper.TwoPi * i / burstCount).ToRotationVector2();
			Vector2 position = npc.Center + direction * Main.rand.NextFloat(12f, Math.Max(npc.width, npc.height) * 0.75f + 24f);
			Vector2 velocity = direction * Main.rand.NextFloat(1.8f, 5.2f);
			Dust dust = Dust.NewDustPerfect(position, dustType, velocity, 30, color, Main.rand.NextFloat(1f, 1.6f));
			dust.noGravity = true;
		}

		for (int i = 0; i < 12; i++)
		{
			Vector2 velocity = Main.rand.NextVector2CircularEdge(4.5f, 4.5f);
			Dust dust = Dust.NewDustPerfect(npc.Center, DustID.Enchanted_Gold, velocity, 40, color, Main.rand.NextFloat(0.9f, 1.35f));
			dust.noGravity = true;
		}
	}

	private static int GetDustType(RuneboundFamily family)
	{
		return family switch
		{
			RuneboundFamily.Vigor => DustID.Blood,
			RuneboundFamily.Bastion => DustID.Stone,
			RuneboundFamily.Embers => DustID.Torch,
			RuneboundFamily.Rime => DustID.Ice,
			RuneboundFamily.Tempests => DustID.Electric,
			RuneboundFamily.Void => DustID.Shadowflame,
			RuneboundFamily.Might => DustID.RedTorch,
			RuneboundFamily.Precision => DustID.GoldFlame,
			RuneboundFamily.Haste => DustID.GreenTorch,
			RuneboundFamily.Spirit => DustID.BlueTorch,
			_ => DustID.Enchanted_Gold,
		};
	}

	private static Color GetSecondaryColor(RuneboundFamily family)
	{
		return family switch
		{
			RuneboundFamily.Vigor => new Color(255, 174, 160),
			RuneboundFamily.Bastion => new Color(245, 226, 180),
			RuneboundFamily.Embers => new Color(255, 224, 84),
			RuneboundFamily.Rime => new Color(221, 250, 255),
			RuneboundFamily.Tempests => new Color(245, 250, 255),
			RuneboundFamily.Void => new Color(238, 109, 255),
			RuneboundFamily.Might => new Color(255, 151, 73),
			RuneboundFamily.Precision => Color.White,
			RuneboundFamily.Haste => new Color(184, 255, 218),
			RuneboundFamily.Spirit => new Color(159, 209, 255),
			_ => Color.White,
		};
	}
}
