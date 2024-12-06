namespace PathOfTerraria.Content.Projectiles.Utility;

internal class ExplosionHitbox : ModProjectile
{
	public override string Texture => "Terraria/Images/NPC_0";

	private ref float Width => ref Projectile.ai[0];
	private ref float Height => ref Projectile.ai[1];

	public override void SetDefaults()
	{
		Projectile.friendly = true;
		Projectile.hostile = true;
		Projectile.timeLeft = 3;
		Projectile.tileCollide = false;
		Projectile.Size = new Vector2(100, 100);
		Projectile.Opacity = 0.5f;
		Projectile.netImportant = true;
		Projectile.penetrate = -1;
	}

	public override void AI()
	{
		if (Projectile.width != (int)Width || Projectile.height != (int)Height)
		{
			Projectile.Resize((int)Width, (int)Height);
		}
	}
}

internal class ExplosionHitboxFriendly : ExplosionHitbox
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Projectile.hostile = false;
	}
}