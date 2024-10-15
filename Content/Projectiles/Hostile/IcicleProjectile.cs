using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Hostile;

public class IcicleProjectile : ModProjectile
{
	public override void SetStaticDefaults()
	{
		Main.projFrames[Type] = 3;
	}

	public override void SetDefaults()
	{
		Projectile.aiStyle = -1;
		Projectile.width = 16;
		Projectile.height = 32;
		Projectile.friendly = true;
		Projectile.hostile = true;
		Projectile.Opacity = 1;
	}

	public override void AI()
	{
		Projectile.velocity.Y += 0.2f;

		if (Main.rand.NextBool(10))
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ice, 0, Projectile.velocity.Y * 0.5f);
		}

		for (int i = -1; i < 2; ++i)
		{
			Point killIcePos = (Projectile.Center + new Vector2(0, 30)).ToTileCoordinates();
			killIcePos.X += i;

			if (Main.tile[killIcePos].TileType == TileID.BreakableIce)
			{
				WorldGen.KillTile(killIcePos.X, killIcePos.Y, false);
			}
		}
	}

	public override void OnKill(int timeLeft)
	{
		for (int i = 0; i < 12; ++i)
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ice);
		}
	}
}