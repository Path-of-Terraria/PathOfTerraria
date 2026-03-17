namespace PathOfTerraria.Content.Projectiles.PassiveProjectiles;

internal class StickyFlame : ModProjectile
{
	public const int MaxTimeLeft = 1200;

	private bool Stuck
	{
		get => Projectile.ai[0] == 1;
		set => Projectile.ai[0] = value ? 1 : 0;
	}

	private ref float TimeReduction => ref Projectile.ai[1];

	public override void SetStaticDefaults()
	{
		Main.projFrames[Type] = 4;
	}

	public override void SetDefaults()
	{
		Projectile.Size = new(20);
		Projectile.timeLeft = MaxTimeLeft;
		Projectile.hostile = false;
		Projectile.friendly = true;
		Projectile.aiStyle = -1;
		Projectile.extraUpdates = 1;
		Projectile.penetrate = -1;
	}

	public override void AI()
	{
		if (TimeReduction > 0)
		{
			Projectile.timeLeft++;
			TimeReduction--;
		}

		if (!Stuck)
		{
			Projectile.velocity.Y += 0.02f;
			Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

			if (Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
			{
				Stuck = true;
			}
		}
		else
		{
			Projectile.velocity = Vector2.Zero;
		}

		Projectile.frame = (int)(Projectile.frameCounter++ * 0.15f % Main.projFrames[Type]);
		Projectile.Opacity = MathHelper.Clamp(Projectile.timeLeft / 60f, 0, 1);
		Lighting.AddLight(Projectile.Center, new Vector3(0.8f, 0.2f, 0) * Projectile.Opacity);
	}

	public override bool OnTileCollide(Vector2 oldVelocity)
	{
		Stuck = true;
		return false;
	}

	public override Color? GetAlpha(Color lightColor)
	{
		return Color.White * Projectile.Opacity;
	}
}
