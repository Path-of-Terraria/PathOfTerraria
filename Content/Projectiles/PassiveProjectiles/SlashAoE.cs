namespace PathOfTerraria.Content.Projectiles.PassiveProjectiles;

internal class SlashAoE : ModProjectile
{
	const int LifeTime = 6 * 6;

	public override void SetStaticDefaults()
	{
		Main.projFrames[Type] = 6;
	}

	public override void SetDefaults()
	{
		Projectile.Size = new(180);
		Projectile.timeLeft = LifeTime;
		Projectile.hostile = false;
		Projectile.friendly = true;
		Projectile.aiStyle = -1;
		Projectile.tileCollide = false;
		Projectile.penetrate = -1;
		Projectile.usesLocalNPCImmunity = true;
		Projectile.localNPCHitCooldown = 25;
		Projectile.rotation = Main.rand.NextFloat(MathF.Tau);
	}

	public override void AI()
	{
		Projectile.frame = (int)((LifeTime - Projectile.timeLeft) / 6f);
	}
}
