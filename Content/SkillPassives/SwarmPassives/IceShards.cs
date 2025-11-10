using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Content.Skills.Summon;

namespace PathOfTerraria.Content.SkillPassives.SwarmPassives;

internal class IceShards : SkillProjectile<Swarm>
{
	public override void SetStaticDefaults()
	{
		Main.projFrames[Type] = 3;
	}

	public override void SetDefaults()
	{
		Projectile.Size = new Vector2(20);
		Projectile.friendly = true;
		Projectile.hostile = false;
		Projectile.aiStyle = -1;
		Projectile.timeLeft = 600;
		Projectile.penetrate = 5;
		Projectile.frame = Main.rand.Next(3);
	}

	public override void AI()
	{
		Projectile.velocity.Y += 0.2f;
		Projectile.rotation += Projectile.velocity.X * 0.07f;

		if (Projectile.timeLeft < 60)
		{
			Projectile.Opacity = Projectile.timeLeft / 60f;
		}
	}

	public override bool OnTileCollide(Vector2 oldVelocity)
	{
		if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
		{
			Projectile.velocity.X = -oldVelocity.X;
		}

		if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
		{
			Projectile.velocity.X *= 0.99f;
		}

		return false;
	}
}