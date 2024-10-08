using PathOfTerraria.Content.Tiles.BossDomain;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Hostile;

public class FallingPusBlock : ModProjectile
{
	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.FallingBlockTileItem[Type] = new ProjectileID.Sets.FallingBlockTileItemInfo(ModContent.TileType<PusBlock>(), 0);
	}

	public override void SetDefaults()
	{
		Projectile.aiStyle = 0;
		Projectile.width = 16;
		Projectile.height = 16;
		Projectile.friendly = true;
		Projectile.hostile = true;
		Projectile.Opacity = 1;
		Projectile.penetrate = -1;
	}

	public override void AI()
	{
		Projectile.velocity.Y += 0.5f;

		if (Main.rand.NextBool(10))
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.YellowStarfish, 0, Projectile.velocity.Y * 0.5f);
		}
	}

	public override void OnKill(int timeLeft)
	{
		Point16 pos = (Projectile.Center + Vector2.Normalize(Projectile.velocity) * 16).ToTileCoordinates16();

		for (int i = -1; i < 2; ++i)
		{
			WorldGen.PlaceTile(pos.X + i, pos.Y, ProjectileID.Sets.FallingBlockTileItem[Type].TileType, true, true);
		}
	}
}