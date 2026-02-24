using Terraria.ID;

namespace PathOfTerraria.Content.Swamp.NPCs.SwampBoss;

internal class MosslingPoison : ModProjectile
{
	private ref float Timer => ref Projectile.ai[0];

	public override void SetDefaults()
	{
		Projectile.CloneDefaults(ProjectileID.Bullet);
		Projectile.aiStyle = -1;
		Projectile.Size = new Vector2(18);
		Projectile.timeLeft = 600;
		Projectile.Opacity = 1f;
		Projectile.friendly = false;
		Projectile.hostile = true;
		Projectile.light = 0;
	}

	public override void AI()
	{
		Timer++;

		Projectile.rotation += Projectile.velocity.X * 0.04f;

		if (Projectile.timeLeft < 30f)
		{
			Projectile.Opacity = Projectile.timeLeft / 30f;
		}

		if (Timer > 360)
		{
			Projectile.velocity.X *= 0.97f;
			Projectile.velocity.Y += 0.02f;

			if (Projectile.velocity.Y > 8)
			{
				Projectile.velocity.Y = 8;
			}
		}

		if (WorldGen.genRand.NextBool(10))
		{
			int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Venom, Projectile.velocity.X, Projectile.velocity.Y, Scale: Main.rand.NextFloat(2, 3));
			Main.dust[dust].noGravity = true;
		}
	}

	public override void OnKill(int timeLeft)
	{
		for (int i = 0; i < 12; ++i)
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Venom);
		}
	}

	public override Color? GetAlpha(Color lightColor)
	{
		return Color.Lerp(lightColor, Color.White, 0.1f) * Projectile.Opacity;
	}
}
