using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Hostile;

public class FallingSlimeProj : ModProjectile
{
	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.FallingBlockTileItem[Type] = new ProjectileID.Sets.FallingBlockTileItemInfo(TileID.SlimeBlock, 0);
	}

	public override void SetDefaults()
	{
		Projectile.aiStyle = ProjAIStyleID.FallingTile;
		Projectile.width = 16;
		Projectile.height = 16;
		Projectile.friendly = true;
		Projectile.hostile = true;
		Projectile.Opacity = 1;
		Projectile.penetrate = -1;
	}

	public override void AI()
	{
		Projectile.ai[0] = 1; // This all stops it from being controlled like a Dirt Rod projectile, which is default functionality for some reason
		Projectile.velocity.X = 0;
		Projectile.velocity.Y += Projectile.ai[0];

		if (Main.rand.NextBool(10))
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.t_Slime, 0, Projectile.velocity.Y * 0.5f);
		}
	}
}