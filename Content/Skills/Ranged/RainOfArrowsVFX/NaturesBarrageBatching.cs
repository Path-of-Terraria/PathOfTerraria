using log4net.Util;
using PathOfTerraria.Content.SkillSpecials.RainOfArrowsSpecials;
using ReLogic.Content;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Skills.Ranged.RainOfArrowsVFX;

/// <summary>
/// Handles batching <see cref="NaturesBarrage"/> poison-tipped arrows for performance.
/// </summary>
internal class NaturesBarrageBatching : ModSystem
{
	public enum ArrowRainShaderType
	{
		Barrage,
		Explosive
	}

	public static bool Batching { get; private set; } = false;
	public static Dictionary<ArrowRainShaderType, Queue<int>> Projectiles = [];

	private static Asset<Effect> PoisonTipEffect;

	public static void AddCache(ArrowRainShaderType type, int who)
	{
		Projectiles.TryAdd(type, []);
		Projectiles[type].Enqueue(who);
	}

	public override void Load()
	{
		On_Main.DrawProjectiles += DrawSunspots;

		PoisonTipEffect = ModContent.Request<Effect>($"{PoTMod.ModName}/Assets/Effects/PoisonTipping");
	}

	private void DrawSunspots(On_Main.orig_DrawProjectiles orig, Main self)
	{
		orig(self);
		
		DrawShaderPass(ArrowRainShaderType.Barrage);
		DrawShaderPass(ArrowRainShaderType.Explosive);
	}

	private static void DrawShaderPass(ArrowRainShaderType type)
	{
		if (!Projectiles.TryGetValue(type, out Queue<int> projectiles) || projectiles.Count <= 0)
		{
			return;
		}

		Matrix trans = Main.GameViewMatrix.TransformationMatrix;
		Effect effect = PoisonTipEffect.Value;
		Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, effect, trans);

		Batching = true;

		while (projectiles.Count > 0)
		{
			int proj = projectiles.Dequeue();
			Projectile projectile = Main.projectile[proj];
			effect.Parameters["target"].SetValue(projectile.GetGlobalProjectile<RainOfArrows.RainProjectile>().PoisonOrientation);
			Main.instance.DrawProjDirect(projectile);
		}

		Batching = false;
		Main.spriteBatch.End();
	}

	public static Vector2 GetTargetBasedOnNormalizedVelocity(Vector2 vel)
	{
		if (GetDotProduct(vel, new Vector2(1, 0), out float side))
		{
			return new Vector2(1f, side);
		}
		else if (GetDotProduct(vel, new Vector2(0, -1), out side))
		{
			return new Vector2(side, 1);
		}
		else if (GetDotProduct(vel, new Vector2(-1, 0), out side))
		{
			return new Vector2(0, side);
		}
		else
		{
			GetDotProduct(vel, new Vector2(0, 1), out side);
			return new Vector2(side, 1);
		}
	}

	public static bool GetDotProduct(Vector2 vel, Vector2 dir, out float side)
	{
		float maxDist = 0.71f; // equivalent to a little over 1 / sqrt(2) for 45 degree angle 
		float dot = Vector2.Dot(vel, dir);

		if (dot > maxDist)
		{
			side = MathHelper.Lerp(0, 1, Utils.GetLerpValue(maxDist, 1, dot));
			return true;
		}

		side = -1;
		return false;
	}
}
