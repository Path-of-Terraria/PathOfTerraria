using PathOfTerraria.Content.Buffs;
using ReLogic.Content;
using SubworldLibrary;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Content.Swamp.NPCs.SwampBoss;

internal class PoisonShaderFunctionality : ModSystem
{
	public class PoisonFunctionalityPlayer : ModPlayer
	{
		public override void PreUpdate()
		{
			const float Range = Mossmother.PosionAuraRadiusSize;
			const float RangeSq = Range * Range;

			bool hasPoison = false;
			bool desperation = false;

			if (!HasPoison(RangeSq, ref hasPoison, ref desperation))
			{
				return;
			}

			if (hasPoison && (!desperation || !Collision.WetCollision(Player.position, Player.width, 10)))
			{
				Player.AddBuff(ModContent.BuffType<ToxicSmogDebuff>(), 2);

				if (desperation)
				{
					ref int toxic = ref Player.GetModPlayer<ToxicSmogDebuff.ToxicSmogPlayer>().TimeToxic;
					toxic = Math.Max(600, toxic);
					Player.breath -= 3;
				}
			}
		}

		private bool HasPoison(float RangeSq, ref bool hasPoison, ref bool desperation)
		{
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.ModNPC is Mossmother mother)
				{
					Vector2 pos = npc.oldPos[Mossmother.TrailingIndex - 1];
					bool nearBoss = pos.DistanceSQ(Player.Center) < RangeSq;

					if (mother.State == Mossmother.BehaviorState.GasCrawl)
					{
						hasPoison = true;

						if (nearBoss)
						{
							return false;
						}
					}
					else if (mother.State == Mossmother.BehaviorState.IdleInWall)
					{
						if (nearBoss)
						{
							return false;
						}
					}
					else if (mother.State == Mossmother.BehaviorState.Desperation)
					{
						desperation = true;
						hasPoison = true;
					}
				}
			}

			return true;
		}
	}

	private const string EffectKey = "PathOfTerraria:Poison";

	internal static bool PoisonActive => Intensity > 0;

	internal static float Intensity { get; set; }

	private static bool _desperationPhase = false;
	private static float _waterOpacity = 1f;

	public override void Load()
	{
		Asset<Effect> filterShader = Mod.Assets.Request<Effect>("Assets/Effects/PurpleSmog");
		Filters.Scene[EffectKey] = new Filter(new ScreenShaderData(filterShader, "Pass0"), EffectPriority.VeryHigh);

		On_Main.RenderWater += GetWaterTarget;
	}

	private void GetWaterTarget(On_Main.orig_RenderWater orig, Main self)
	{
		orig(self);
	
		Filters.Scene[EffectKey].GetShader().UseImage(Main.waterTarget, 1);
	}

	public override void PreUpdateEntities()
	{
		if (Main.netMode != NetmodeID.Server && (SubworldSystem.Current is SwampArea || Intensity > 0)) // This all needs to happen client-side!
		{
			if (Intensity > 0 && !Filters.Scene[EffectKey].Active)
			{
				Intensity = 0.02f;
				Filters.Scene.Activate(EffectKey);
				Filters.Scene[EffectKey].GetShader().Shader.Parameters["auraPixelSize"].SetValue(Mossmother.PosionAuraRadiusSize);
			}
			else if (Intensity <= 0.01f && Filters.Scene[EffectKey].Active)
			{
				Intensity = 0;
				Filters.Scene[EffectKey].Deactivate();
			}

			CheckPoisonSmog();

			if (Filters.Scene[EffectKey].Active)
			{
				UpdateEffect();
			}
		}
	}

	private static void CheckPoisonSmog()
	{
		int mothers = 0; // Debug
		var positions = new Vector2[6] { -Vector2.One, -Vector2.One, -Vector2.One, -Vector2.One, -Vector2.One, -Vector2.One };
		bool shouldShowPoison = false;
		Vector2 mouseDebugPos = Main.MouseScreen / Main.ScreenSize.ToVector2();

		_desperationPhase = false;
		bool noNpc = true;

		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.ModNPC is Mossmother mother)
			{
				noNpc = false;

				Vector2 scale = (npc.oldPos[Mossmother.TrailingIndex - 1] + npc.Size / 2f - Main.screenPosition) / Main.ScreenSize.ToVector2();
				positions[mothers] = scale;

				if (mother.State == Mossmother.BehaviorState.GasCrawl)
				{
					shouldShowPoison = true;
				}
				else if (mother.State == Mossmother.BehaviorState.Desperation)
				{
					shouldShowPoison = true;
					_desperationPhase = true;
				}

				if (mothers++ >= 3)
				{
					break;
				}
			}
		}

		if (noNpc)
		{
			_desperationPhase = true;
		}

		_waterOpacity = MathHelper.Lerp(_waterOpacity, _desperationPhase ? 0 : 1, 0.02f);

		Filters.Scene[EffectKey].GetShader().Shader.Parameters["bossPositions"].SetValue(positions);

		if (!shouldShowPoison)
		{
			Intensity = MathF.Max(0, Intensity - 0.001f);
		}
		else
		{
			Intensity = MathHelper.Lerp(Intensity, _desperationPhase ? 0.7f : 0.4f, 0.004f);
		}
	}

	private static void UpdateEffect()
	{
		Filters.Scene[EffectKey].GetShader().UseProgress(Main.GameUpdateCount * 0.002f);
		Vector2 direction = Main.screenPosition / Main.ScreenSize.ToVector2();
		direction.X %= 1;
		direction.Y %= 1;
		Filters.Scene[EffectKey].GetShader().UseDirection(direction);
		Filters.Scene[EffectKey].GetShader().UseImage(ModContent.Request<Texture2D>("PathOfTerraria/Assets/Misc/PerlinNoise"));
		Filters.Scene[EffectKey].GetShader().UseIntensity(Intensity);

		float waterHeight = _desperationPhase ? Utils.GetLerpValue(Main.screenPosition.Y / 16f, Main.screenPosition.Y / 16f + Main.screenHeight / 16f, SwampArea.WaterY - 1, true) : 1;
		Filters.Scene[EffectKey].GetShader().Shader.Parameters["waterHeight"].SetValue(waterHeight);
		Filters.Scene[EffectKey].GetShader().Shader.Parameters["waterOpacity"].SetValue(_waterOpacity);
		Filters.Scene[EffectKey].GetShader().Shader.Parameters["hasWater"].SetValue(_desperationPhase);
	}
}
