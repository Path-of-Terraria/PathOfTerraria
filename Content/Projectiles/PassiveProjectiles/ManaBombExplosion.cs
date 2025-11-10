using PathOfTerraria.Content.Projectiles.Utility;

namespace PathOfTerraria.Content.Projectiles.PassiveProjectiles;

internal class ManaBombExplosion : ExplosionHitboxFriendly
{
	protected override bool UseBaseTexture => true;

	public override void SetStaticDefaults()
	{
		Main.projFrames[Type] = 6;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Projectile.timeLeft = 24;
		Projectile.Opacity = 1f;
		Projectile.hide = true;
	}

	public override bool? CanDamage()
	{
		return Projectile.timeLeft > 20;
	}

	public override void AI()
	{
		base.AI();

		Projectile.hide = false;
		Projectile.frame = 6 - Projectile.timeLeft / 4;
		Projectile.velocity *= 0.8f;
		Projectile.rotation = Projectile.ai[2];
	}
}
