using System.Threading;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Melee;

internal class FireStarterProjectile : ModProjectile
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Sword/FireStarterProj";
	private Player Owner => Main.player[Projectile.owner];

	public override void SetDefaults()
	{
		Projectile.width = 40;
		Projectile.height = 76;
		Projectile.aiStyle = 0;
		Projectile.friendly = true;
		Projectile.penetrate = -1;
		Projectile.timeLeft = 20; // Duration of the projectile - should match sword use time
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.ownerHitCheck = true;
		Projectile.manualDirectionChange = true;
	}

	public override void AI()
	{
		Projectile.direction = Owner.direction;

		// Swing arc calculations
		float swingArc = MathHelper.ToRadians(160f); // Swing arc in degrees

		// Calculate swing position and rotation
		float progress = (20f - Projectile.timeLeft) / 20f;
		float rotation = -swingArc / 2 + swingArc * progress;

		// Adjust pivot point to the player's hand position
		Vector2 handOffset = new Vector2(0, -20); // Adjust this based on where you want the pivot to be
		Vector2 offset = new Vector2(Projectile.width / 2, 0).RotatedBy(rotation) * Projectile.direction;
		Projectile.position = Owner.Center + handOffset + offset - new Vector2(Projectile.width / 2, Projectile.height / 2);

		Projectile.rotation = rotation * Projectile.direction;

		// Sync the projectile's position with the player's
		Owner.heldProj = Projectile.whoAmI;
		Owner.itemTime = Owner.itemAnimation = Projectile.timeLeft;

		if (Projectile.timeLeft <= 1)
		{
			Projectile.Kill();
		}
	}
}