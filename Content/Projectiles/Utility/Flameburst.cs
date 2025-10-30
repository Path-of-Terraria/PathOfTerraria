namespace PathOfTerraria.Content.Projectiles.Utility;

internal class Flameburst : ExplosionHitboxFriendly
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		Main.projFrames[Type] = 6;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Projectile.timeLeft = 30;
		Projectile.Opacity = 1f;
	}

	public override bool? CanDamage()
	{
		return Projectile.timeLeft > 22;
	}

	public override string Texture => (GetType().Namespace + "." + Name).Replace('.', '/');

	public override void AI()
	{
		base.AI();

		Projectile.frame = 6 - (Projectile.timeLeft / 5);
	}
}
