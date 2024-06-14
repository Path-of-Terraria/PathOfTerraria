using PathOfTerraria.Content.Items.Gear.Weapons.Bow;
using PathOfTerraria.Core.Systems;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Ranged;

internal abstract class BowAnimationProjectile : ModProjectile
{
	private Player Owner => Main.player[Projectile.owner];
	
	public override void SetStaticDefaults()
	{
		Main.projFrames[Projectile.type] = 6;
	}
	
	public override void SetDefaults()
	{
		Projectile.width = 32;
		Projectile.height = 64;
		Projectile.tileCollide = false;
		Projectile.friendly = false;
	}

	public override void AI()
	{
		// If not holding right, and the post-shoot animation isn't playing, remove the projectile
		if (Main.myPlayer == Projectile.owner && !Main.mouseRight && Projectile.frame is not >= 4 and < 6)
		{
			Projectile.Kill();
		}

		Owner.itemAnimation = Owner.itemTime = 2;
		Owner.SetDummyItemTime(2); // Stop the player from switching items when in use
		Owner.direction = Math.Sign(Owner.DirectionTo(Main.MouseWorld).X);
		Projectile.rotation = Owner.DirectionTo(Main.MouseWorld).ToRotation();
		Projectile.velocity = Vector2.Zero;
		Projectile.Center = Owner.Center + Owner.DirectionTo(Main.MouseWorld) * 20; // Move bow towards mouse so it looks like it's being held
		Owner.itemRotation = Projectile.rotation;

		if (Owner.direction != 1)
		{
			Owner.itemRotation -= 3.14f;
		}

		Owner.heldProj = Projectile.whoAmI;

		Projectile.frameCounter++;

		if (Projectile.frameCounter % 6 == 5)
		{
			if (Projectile.frame < 6)
			{
				Projectile.frame++;
			}

			if (Projectile.frame == 4)
			{
				Shoot();
			}

			if (Projectile.frame == 6)
			{
				Projectile.Kill();
			}
		}

		Player.CompositeArmStretchAmount stretch = Projectile.frame switch
		{
			3 => Player.CompositeArmStretchAmount.None,
			2 => Player.CompositeArmStretchAmount.Quarter,
			1 => Player.CompositeArmStretchAmount.ThreeQuarters,
			_ => Player.CompositeArmStretchAmount.Full
		};

		Owner.SetCompositeArmFront(true, stretch, Projectile.rotation - 1.57f);
	}
	
	private void Shoot()
	{
		if (Owner.whoAmI != Main.myPlayer && Owner.HeldItem.ModItem is not Bow bow)
		{
			return;
		}

		Terraria.Audio.SoundEngine.PlaySound(SoundID.Item5, Projectile.Center);
		Owner.PickAmmo(Owner.HeldItem, out int type, out float speed, out int damage, out float kB, out int ammoUsed);
		Owner.GetModPlayer<AltUseSystem>().AltFunctionCooldown = 5 * 60;

		damage = (int)(damage * 3f);
		Vector2 vel = Projectile.DirectionTo(Main.MouseWorld) * speed * 1.5f;
		Projectile.NewProjectile(Owner.GetSource_ItemUse_WithPotentialAmmo(Owner.HeldItem, ammoUsed), Projectile.Center, vel, ProjectileID.WoodenArrowFriendly, damage, kB, Owner.whoAmI);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		// Manually draw the projectile so it aligns properly when rotated
		Texture2D tex = TextureAssets.Projectile[Type].Value;
		int frameHeight = tex.Height / Main.projFrames[Type];
		var src = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);

		Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition - new Vector2(0, 10), src, Color.White, Projectile.rotation, src.Size() / 2f, 1f, SpriteEffects.None, 0);
		return false;
	}
}