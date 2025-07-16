using PathOfTerraria.Content.SkillPassives.RainOfArrowsTree;
using ReLogic.Content;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Skills.Ranged.RainOfArrowsVFX;

/// <summary>
/// Handles batching <see cref="NaturesBarrage"/> poison-tipped arrows for performance.
/// </summary>
internal class NaturesBarrageBatching : ModSystem
{
	public static bool Batching { get; private set; } = false;
	public static Queue<int> Projectiles = new();

	private static Asset<Effect> PoisonTipEffect;

	public override void Load()
	{
		On_Main.DrawProjectiles += DrawSunspots;

		PoisonTipEffect = ModContent.Request<Effect>($"{PoTMod.ModName}/Assets/Effects/PoisonTipping");
	}

	private void DrawSunspots(On_Main.orig_DrawProjectiles orig, Main self)
	{
		orig(self);

		if (Projectiles.Count <= 0)
		{
			return;
		}

		Matrix trans = Main.GameViewMatrix.TransformationMatrix;
		Effect effect = PoisonTipEffect.Value;
		Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, effect, trans);

		Batching = true;

		while (Projectiles.Count > 0)
		{
			int proj = Projectiles.Dequeue();
			Projectile projectile = Main.projectile[proj];
			effect.Parameters["target"].SetValue(projectile.GetGlobalProjectile<RainOfArrows.RainProjectile>().PoisonOrientation);
			Main.instance.DrawProjDirect(projectile);
		}

		Batching = false;
		Main.spriteBatch.End();
	}

	public static Vector2 GetTargetBasedOnRotation(float rotation)
	{
		rotation %= MathHelper.Pi;

		if (rotation >= -MathHelper.PiOver4 && rotation <= MathHelper.PiOver4)
		{
			return new Vector2(MathHelper.Lerp(0, 1, Utils.GetLerpValue(-MathHelper.PiOver4, MathHelper.PiOver4, rotation)), 0);
		}
		else if (rotation <= -MathHelper.PiOver4 && rotation >= -3 * MathHelper.PiOver4 / 4)
		{
			return new Vector2(0, MathHelper.Lerp(0, 1, Utils.GetLerpValue(-3 * MathHelper.PiOver4 / 4, -MathHelper.PiOver4, rotation)));
		}
		else if (rotation >= MathHelper.PiOver4 && rotation <= 3 * MathHelper.PiOver4 / 4)
		{
			return new Vector2(1, MathHelper.Lerp(1, 0, Utils.GetLerpValue(MathHelper.PiOver4, 3f * MathHelper.Pi / 4f, rotation)));
		}
		else
		{
			if (rotation <= -3 * MathHelper.PiOver4 / 4)
			{
				return new Vector2(MathHelper.Lerp(0, 0.5f, Utils.GetLerpValue(-3 * MathHelper.PiOver4 / 4f, -MathHelper.Pi, rotation)), 1);
			}
			else
			{
				return new Vector2(MathHelper.Lerp(0.5f, 1f, Utils.GetLerpValue(3 * MathHelper.PiOver4 / 4f, MathHelper.Pi, rotation)), 1);
			}
		}
	}

	public static Vector2 GetTargetBasedOnNormalizedVelocity(Vector2 vel)
	{
		if (GetDotProduct(vel, new Vector2(1, 0), out float side)) // closest to facing right
		{
			return new Vector2(1f, side);
		}
		else if (GetDotProduct(vel, new Vector2(0, -1), out side)) //closest to facing up
		{
			return new Vector2(side, 1);
		}
		else if (GetDotProduct(vel, new Vector2(-1, 0), out side)) // closest to facing left
		{
			return new Vector2(0, side);
		}
		else //closest to facing down
		{
			GetDotProduct(vel, new Vector2(0, 1), out side);
			return new Vector2(side, 1);
		}
	}

	public static bool GetDotProduct(Vector2 vel, Vector2 dir, out float side)
	{
		float maxDist = 0.71f; // equivalent to a little over 1 / sqrt(2) for 45 degree angle 
		float dot = Vector2.Dot(vel, dir);

		if (dot > maxDist) // closest to facing right
		{
			side = MathHelper.Lerp(0, 1, Utils.GetLerpValue(maxDist, 1, dot));
			return true;
		}

		side = -1;
		return false;
	}
}
