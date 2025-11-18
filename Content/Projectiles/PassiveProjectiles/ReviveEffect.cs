namespace PathOfTerraria.Content.Projectiles.PassiveProjectiles;

internal class ReviveEffect : ModProjectile
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		Main.projFrames[Type] = 7;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Projectile.timeLeft = 49;
		Projectile.Opacity = 1f;
		Projectile.Size = new Vector2(32, 52);
	}

	public override bool? CanDamage()
	{
		return false;
	}

	public override void AI()
	{
		base.AI();

		Projectile.frame = 7 - Projectile.timeLeft / 7;
		Projectile.velocity.Y *= 0.95f;
	}
}
