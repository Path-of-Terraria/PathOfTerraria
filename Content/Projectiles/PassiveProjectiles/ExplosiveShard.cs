using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.PassiveProjectiles;

internal class ExplosiveShard : ModProjectile
{
	public override void SetStaticDefaults()
	{
		Main.projFrames[Type] = 3;
	}

	public override void SetDefaults()
	{
		Projectile.Size = new(12);
		Projectile.timeLeft = 120;
		Projectile.hostile = false;
		Projectile.friendly = true;
		Projectile.aiStyle = -1;
		Projectile.frame = Main.rand.Next(3);
		Projectile.extraUpdates = 1;
	}

	public override void AI()
	{
		if (Main.rand.NextBool(30))
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, Projectile.velocity.X, Projectile.velocity.Y);
		}

		Projectile.velocity *= 0.97f;
		Projectile.velocity.Y += 0.02f;
		Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

		if (Projectile.velocity.LengthSquared() < 2f)
		{
			Projectile.Kill();
		}
	}
}
