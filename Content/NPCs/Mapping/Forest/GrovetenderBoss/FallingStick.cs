using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Forest.GrovetenderBoss;

internal class FallingStick : ModProjectile
{
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
		Projectile.extraUpdates = 1;
		Projectile.tileCollide = false;
	}

	public override void AI()
	{
		Projectile.rotation += Projectile.velocity.Y * 0.005f;
		Projectile.velocity.Y += 0.1f;

		if (Projectile.timeLeft < 19 * 60)
		{
			Projectile.tileCollide = true;
		}

		if (Main.rand.NextBool(20))
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WoodFurniture, Projectile.velocity.X, Projectile.velocity.Y);
		}
	}

	public override void OnKill(int timeLeft)
	{
		for (int i = 0; i < 12; ++i)
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WoodFurniture, Projectile.velocity.X, Projectile.velocity.Y);
		}
	}
}