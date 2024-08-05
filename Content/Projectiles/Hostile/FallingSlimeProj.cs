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
	}

	public override void AI()
	{
		Projectile.velocity.Y += Projectile.ai[0];

		if (Main.rand.NextBool(10))
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.t_Slime, 0, Projectile.velocity.Y * 0.5f);
		}
	}
}