using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.PassiveProjectiles;

internal class BlastIcicle : ModProjectile
{
	public const int MaxTimeLeft = 120;

	private ref float ScaleModifier => ref Projectile.ai[0];

	public override void SetStaticDefaults()
	{
		Main.projFrames[Type] = 3;
	}

	public override void SetDefaults()
	{
		Projectile.Size = new(16);
		Projectile.timeLeft = MaxTimeLeft;
		Projectile.hostile = false;
		Projectile.friendly = true;
		Projectile.aiStyle = -1;
		Projectile.frame = Main.rand.Next(3);
		Projectile.extraUpdates = 1;
	}

	public override void AI()
	{
		if (Projectile.timeLeft == MaxTimeLeft)
		{
			Projectile.Size *= ScaleModifier;
			Projectile.scale *= ScaleModifier;
		}

		if (Main.rand.NextBool(36))
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ice, Projectile.velocity.X, Projectile.velocity.Y);
		}

		Projectile.velocity *= 0.98f;
		Projectile.velocity.Y += 0.05f;
		Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		Projectile.Opacity = MathF.Min(MathF.Min(Projectile.timeLeft, (Projectile.velocity.Length() - 1.5f) * 8f) / 10f, 1);

		if (Projectile.velocity.LengthSquared() < 1.5f)
		{
			Projectile.Kill();
		}
	}
}

internal class BlastIcicleSmall : BlastIcicle
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Projectile.Size = new Vector2(10);
	}
}