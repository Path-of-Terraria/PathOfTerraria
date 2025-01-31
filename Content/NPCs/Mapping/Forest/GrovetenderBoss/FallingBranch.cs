using Terraria.Audio;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Forest.GrovetenderBoss;

internal class FallingBranch : ModProjectile
{
	private ref float WaitTimer => ref Projectile.ai[0];

	public override void SetStaticDefaults()
	{
		Main.projFrames[Type] = 3;
	}

	public override void SetDefaults()
	{
		Projectile.aiStyle = -1;
		Projectile.timeLeft = 20 * 60;
		Projectile.hostile = true;
		Projectile.friendly = false;
		Projectile.Size = new(28);
		Projectile.penetrate = -1;
		Projectile.tileCollide = false;
		Projectile.frame = Main.rand.Next(3);
	}

	public override bool CanHitPlayer(Player target)
	{
		return WaitTimer <= 0;
	}

	public override void AI()
	{
		if (WaitTimer-- > 0)
		{
			if (WaitTimer > 100)
			{
				var velocity = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(4, 10));
				int type = DustID.Grass;
				Dust.NewDustPerfect(Projectile.Center, type, velocity, 0, default, Main.rand.NextFloat(1.5f, 2.5f));
			}

			return;
		}

		if (WaitTimer > -30)
		{
			Projectile.Opacity = -WaitTimer / 30;
		}

		Projectile.rotation += Projectile.velocity.Y * 0.005f;
		Projectile.velocity.Y += 0.1f;

		if (!Projectile.tileCollide && !Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
		{
			Projectile.tileCollide = true;
		}

		if (Main.rand.NextBool(20))
		{
			int type = !Main.rand.NextBool(3) ? DustID.Grass : DustID.WoodFurniture;
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, type, Projectile.velocity.X, Projectile.velocity.Y);
		}
	}

	public override bool ShouldUpdatePosition()
	{
		return WaitTimer <= 0;
	}

	public override void OnKill(int timeLeft)
	{
		for (int i = 0; i < 12; ++i)
		{
			int type = !Main.rand.NextBool(3) ? DustID.Grass : DustID.WoodFurniture;
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, type, Projectile.velocity.X, Projectile.velocity.Y);
		}

		SoundEngine.PlaySound(SoundID.Grass, Projectile.Center);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		return WaitTimer <= -2;
	}
}