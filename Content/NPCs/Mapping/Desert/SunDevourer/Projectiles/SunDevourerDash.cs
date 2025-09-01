namespace PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer.Projectiles;

public sealed class SunDevourerDash : ModProjectile
{
	public NPC Parent => Main.npc[(int)ParentWho];

	public ref float LifeTime => ref Projectile.ai[0];
	public ref float Timer => ref Projectile.ai[1];
	public ref float ParentWho => ref Projectile.ai[2];

	public override void SetStaticDefaults()
	{
		Main.projFrames[Type] = 4;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Projectile.friendly = false;
		Projectile.hostile = true;
		Projectile.tileCollide = false;

		Projectile.width = 178;
		Projectile.height = 256;
		Projectile.timeLeft = 2;
		Projectile.Opacity = 0f;
	}

	public override bool OnTileCollide(Vector2 oldVelocity)
	{
		return false;
	}

	public override void AI()
	{
		Projectile.rotation = Parent.rotation;
		Projectile.spriteDirection = Parent.spriteDirection;
		Projectile.Center = Parent.Center + Parent.rotation.ToRotationVector2() * 84;
		Projectile.timeLeft = 2;

		if (Parent.spriteDirection == -1)
		{
			Projectile.Center = Parent.Center + (Parent.rotation - MathHelper.Pi).ToRotationVector2() * 84;
		}

		Timer++;

		Projectile.frame = (int)(Timer * 0.4f) % 4;

		if (Timer > LifeTime)
		{
			Projectile.Kill();
			return;
		}

		if (Timer <= 15)
		{
			Projectile.Opacity = Timer / 15f;
		}
		else if (Timer > LifeTime - 15f)
		{
			Projectile.Opacity = 1 - (Timer - (LifeTime - 15f)) / 15f;
		}
	}
}