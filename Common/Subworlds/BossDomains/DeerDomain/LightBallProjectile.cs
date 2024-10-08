using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.DeerDomain;

internal class LightBallProjectile : ModProjectile
{
	public override string Texture => "Terraria/Images/NPC_0";

	private ref float Timer => ref Projectile.ai[0];
	private ref float PassThroughTimer => ref Projectile.ai[1];

	public override void SetDefaults()
	{
		Projectile.CloneDefaults(ProjectileID.Bullet);
		Projectile.Opacity = 0;
		Projectile.aiStyle = 0;
		Projectile.timeLeft = 2;
		Projectile.hostile = true;
		Projectile.friendly = true;
		Projectile.tileCollide = false;
		Projectile.Size = new(16);
	}

	public override void AI()
	{
		if (Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
		{
			PassThroughTimer++;

			if (PassThroughTimer > 600)
			{
				Projectile.Kill();
				return;
			}
		}
		else
		{
			PassThroughTimer--;
			PassThroughTimer = Math.Max(PassThroughTimer, 0);
		}

		Projectile.Opacity += 0.025f;
		Projectile.timeLeft++;
		Projectile.velocity.Y = MathF.Sin(Timer++ * 0.02f) * 0.2f;

		if (Projectile.Opacity > 1)
		{
			Projectile.Opacity = 1;
		}

		Projectile.Opacity = 1 - PassThroughTimer / 600f;
		DeerclopsDomain.LightMultiplier = 1f;
		Lighting.AddLight(Projectile.Center, new Vector3(1.2f, 1f, 1.25f) * Projectile.Opacity);
		DeerclopsDomain.LightMultiplier = 0f;

		Player closest = Main.player[Player.FindClosest(Projectile.Center, 2, 2)];
		float xDist = MathHelper.Clamp(Math.Abs(closest.Center.X - Projectile.Center.X ) / 200f, 0, 1) * 0.4f;
		int velDir = Math.Sign(Projectile.velocity.X);

		if (closest.Center.X < Projectile.Center.X)
		{
			ModifyVelocity(xDist, velDir);
		}
		else
		{
			ModifyVelocity(-xDist, velDir);
		}
	}

	private void ModifyVelocity(float xDist, int velDir)
	{
		if (velDir == 1)
		{
			Projectile.position.X -= MathHelper.Lerp(Projectile.velocity.X * xDist, 0, 0.9f);
		}
		else
		{
			Projectile.position.X += Projectile.velocity.X * xDist;
		}
	}

	public override void PostDraw(Color lightColor)
	{
		Color color = Color.Lerp(Color.Red, Color.White, 0.4f) with { A = 0 } * Projectile.Opacity;
		DrawGlow(Projectile.Center, Projectile.whoAmI, color);
	}

	public static void DrawGlow(Vector2 position, float sineOffset, Color baseColor, bool isTile = false)
	{
		Vector2 offScreen = Main.drawToScreen || !isTile ? Vector2.Zero : new Vector2(Main.offScreenRange);
		Texture2D tex = DeerclopsDomain.LightGlow.Value;
		Vector2 pos = position - Main.screenPosition + offScreen;
		float rotation = MathF.Sin((sineOffset + Main.GameUpdateCount) * 0.02f);
		Main.spriteBatch.Draw(tex, pos, null, baseColor * 0.7f, rotation, tex.Size() / 2f, 1f, SpriteEffects.None, 0);
		Main.spriteBatch.Draw(tex, pos, null, baseColor * 0.3f, -rotation + MathHelper.PiOver2, tex.Size() / 2f, 1.2f, SpriteEffects.None, 0);
	}
}
