using PathOfTerraria.Common.Systems;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Melee;

internal class FireStarterProjectile : ModProjectile
{
	public override string Texture => $"{PoTMod.ModName}/Assets/Items/Gear/Weapons/Sword/FireStarterProj";
	private Player Owner => Main.player[Projectile.owner];

	public override void SetDefaults()
	{
		Projectile.width = 50;
		Projectile.height = 50;
		Projectile.aiStyle = -1;
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
		float rotation = GetCurrentSwingRotation();

		if (Owner.direction == -1)
		{
			rotation *= -1;
			rotation += MathHelper.PiOver2 * 3.2f;
		}

		// Adjust pivot point to the player's hand position
		Projectile.Center = Owner.HandPosition ?? Vector2.Zero;
		Projectile.rotation = rotation;

		// Set the owner's information to match that of this projectile
		Owner.heldProj = Projectile.whoAmI;
		Owner.itemTime = Owner.itemAnimation = Projectile.timeLeft;

		if (!Main.rand.NextBool(3)) // Spawn dust
		{
			Vector2 dustPos = Vector2.Lerp(Projectile.Center, GetProjectileTip(), Main.rand.NextFloat());
			Dust.NewDust(dustPos, 1, 1, DustID.Torch);
		}
	}

	private float GetCurrentSwingRotation()
	{
		// Swing arc calculations
		float swingArc = MathHelper.ToRadians(160f); // Swing arc in degrees

		// Calculate swing position and rotation
		float progress = (20f - Projectile.timeLeft) / 20f;
		float rotation = -swingArc / 2 + swingArc * progress;
		return rotation;
	}

	public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
	{
		// Makes the hitbox much more accurate to the swung sword, and stops us from needing to position the hitbox alongside the sprite properly
		return Collision.CheckAABBvLineCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size(), Projectile.Center, GetProjectileTip());
	}

	private Vector2 GetProjectileTip()
	{
		// Note that the PiOver2 adjusts since the swing rotation is slightly off compared to the sprite, since the 
		return Projectile.Center + (GetCurrentSwingRotation() - MathHelper.PiOver4).ToRotationVector2() * 70;
	}

	public override bool PreDraw(ref Color lightColor)
	{
		float rotation = Projectile.rotation;
		Vector2 pos = Projectile.Center - Main.screenPosition;

		Main.spriteBatch.Draw(TextureAssets.Projectile[Type].Value, pos, null, Color.White, rotation, new Vector2(0, 50), 1f, SpriteEffects.None, 0);
		return false;
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		AltUsePlayer modPlayer = Owner.GetModPlayer<AltUsePlayer>();

		if (modPlayer.AltFunctionActive)
		{
			target.AddBuff(BuffID.OnFire, 180);
		}

		base.OnHitNPC(target, hit, damageDone);
	}
}