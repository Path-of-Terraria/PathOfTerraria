namespace PathOfTerraria.Content.Projectiles.Melee;

internal class FireStarterProjectile : ModProjectile
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Sword/FireStarterProj";

	public override void SetDefaults()
	{
		Projectile.width = 40;
		Projectile.height = 76;
		Projectile.aiStyle = 0;
		Projectile.friendly = false;
		Projectile.penetrate = -1;
		Projectile.timeLeft = 20; // Duration of the projectile - should match sword use time
		Projectile.tileCollide = false;
	}
}