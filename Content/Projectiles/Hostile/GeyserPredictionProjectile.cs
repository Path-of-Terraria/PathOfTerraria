using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Hostile;

internal class GeyserPredictionProjectile : ModProjectile
{
	public override string Texture => "Terraria/Images/NPC_0";

	public override void SetDefaults()
	{
		Projectile.CloneDefaults(ProjectileID.PurificationPowder);
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.aiStyle = -1;
		Projectile.timeLeft = 80;
	}

	public override void AI()
	{
		int dustType = Utils.SelectRandom(Main.rand, 6, 259, 158);
		Dust.NewDustPerfect(Projectile.Center + new Vector2(0, 10), dustType, new Vector2(Main.rand.NextFloat(-2, 2), -Main.rand.NextFloat(2, 6)));

		if (Projectile.timeLeft == 1 && Main.netMode != NetmodeID.MultiplayerClient)
		{
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(0, -2), ProjectileID.GeyserTrap, 30, 0.1f, Main.myPlayer, 1);
		}
	}
}
