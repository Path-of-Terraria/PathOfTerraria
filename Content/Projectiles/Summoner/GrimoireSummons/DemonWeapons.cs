using Terraria.Audio;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Summoner.GrimoireSummons;

public class DemonWeapons : ModProjectile
{
	public override void SetStaticDefaults()
	{
		Main.projFrames[Type] = 3;
	}

	public override void SetDefaults()
	{
		Projectile.CloneDefaults(ProjectileID.DemonScythe);
		Projectile.width = Projectile.height = 26;
		Projectile.friendly = true;
		Projectile.hostile = false;
		Projectile.DamageType = DamageClass.Magic;
		Projectile.timeLeft = 600;
		Projectile.frame = Main.rand.Next(3);
	}

	public override void AI()
	{
		if (Main.rand.NextBool(3))
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, Projectile.velocity.X, Projectile.velocity.Y);
		}
	}

	public override void OnKill(int timeLeft)
	{
		for (int k = 0; k < 10; k++)
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, Projectile.velocity.X, Projectile.velocity.Y);
		}

		SoundEngine.PlaySound(SoundID.Item25, Projectile.position);
	}
}