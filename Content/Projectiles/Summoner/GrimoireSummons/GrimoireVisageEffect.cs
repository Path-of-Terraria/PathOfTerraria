namespace PathOfTerraria.Content.Projectiles.Summoner.GrimoireSummons;

internal class GrimoireVisageEffect : ModProjectile
{
	public override string Texture => "PathOfTerraria/Assets/Projectiles/Summoner/GrimoireSummons/GrimoireVisage";

	public override void SetStaticDefaults()
	{
		Main.projFrames[Type] = 7;
	}

	public override void SetDefaults()
	{
		Projectile.friendly = Projectile.hostile = false;
		Projectile.width = 30;
		Projectile.height = 32;
		Projectile.timeLeft = 50;
	}

	public override void AI()
	{
		Projectile.frame = 6 - (int)(7 * (Projectile.timeLeft / 50f));
	}
}
