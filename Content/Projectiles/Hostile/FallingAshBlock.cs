using Terraria.Audio;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Hostile;

public class FallingAshBlock : ModProjectile
{
	public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.AshBallFalling}";

	public override void SetDefaults()
	{
		Projectile.CloneDefaults(ProjectileID.AshBallFalling);
		Projectile.aiStyle = -1;
		Projectile.friendly = true;
		Projectile.hostile = true;
	}

	public override void AI()
	{
		Projectile.velocity.Y += 0.15f;
		Projectile.rotation += 1.4f;
	}

	public override bool? CanCutTiles()
	{
		return false;
	}

	public override void OnKill(int timeLeft)
	{
		for (int i = 0; i < 15; ++i)
		{
			Projectile.velocity *= 0.5f;
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ash, Projectile.velocity.X, Projectile.velocity.Y);
		}

		SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
	}
}