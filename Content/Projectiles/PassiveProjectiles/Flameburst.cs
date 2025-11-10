using PathOfTerraria.Content.Projectiles.Utility;

namespace PathOfTerraria.Content.Projectiles.PassiveProjectiles;

internal class Flameburst : ExplosionHitboxFriendly
{
	protected override bool UseBaseTexture => true;

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

	public override void AI()
	{
		base.AI();

		Projectile.frame = 6 - Projectile.timeLeft / 5;
	}
}
