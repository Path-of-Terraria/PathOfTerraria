using PathOfTerraria.Content.Dusts;
using Terraria.Audio;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Magic;

public sealed class SparklingBallProjectile : ModProjectile
{
	public override string Texture => $"{PoTMod.ModName}/Assets/Projectiles/SparklingBallProjectile";

	public override void SetDefaults()
	{
		Projectile.width = 16;
		Projectile.height = 16;
		Projectile.friendly = true;
		Projectile.DamageType = DamageClass.Magic;
		Projectile.penetrate = 3;
		Projectile.timeLeft = 600;
	}

	public override void AI()
	{
		Projectile.velocity.Y += Projectile.ai[0];

		if (Main.rand.NextBool(3))
		{
			Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, ModContent.DustType<SparkleDust>(), Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
		}
	}

	public override bool OnTileCollide(Vector2 oldVelocity)
	{
		Projectile.penetrate--;

		if (Projectile.penetrate <= 0)
		{
			Projectile.Kill();
		}
		else
		{
			Projectile.ai[0] += 0.1f;
			if (Projectile.velocity.X != oldVelocity.X)
			{
				Projectile.velocity.X = -oldVelocity.X;
			}

			if (Projectile.velocity.Y != oldVelocity.Y)
			{
				Projectile.velocity.Y = -oldVelocity.Y;
			}

			Projectile.velocity *= 0.75f;
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
		}

		return false;
	}

	public override void OnKill(int timeLeft)
	{
		for (int k = 0; k < 5; k++)
		{
			Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, ModContent.DustType<SparkleDust>(), Projectile.oldVelocity.X * 0.5f, Projectile.oldVelocity.Y * 0.5f);
		}

		SoundEngine.PlaySound(SoundID.Item25, Projectile.position);
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		Projectile.ai[0] += 0.1f;
		Projectile.velocity *= 0.75f;
	}
}