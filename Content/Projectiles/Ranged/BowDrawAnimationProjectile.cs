using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Ranged;

internal class BowDrawAnimationProjectile : ModProjectile
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Bow/WoodenBowAnimated";
	private Player Owner => Main.player[Projectile.owner];
	
	public override void SetStaticDefaults()
	{
		Main.projFrames[Projectile.type] = 6;
	}
	
	public override void SetDefaults()
	{
		Projectile.width = 32;
		Projectile.height = 32;
		Projectile.tileCollide = false;
		Projectile.friendly = false;
	}

	public override void AI()
	{
		if (!Owner.channel)
		{
			//Todo: Figure out why channel is false when holding right-click
			//Projectile.active = false;
		}

		Owner.itemAnimation = Owner.itemTime = 2;
		Owner.direction = Math.Sign(Owner.DirectionTo(Main.MouseWorld).X);
		Projectile.rotation = Owner.DirectionTo(Main.MouseWorld).ToRotation();
		Projectile.velocity = Vector2.Zero;
		Projectile.Center = Owner.Center;

		Owner.itemRotation = Projectile.rotation;

		if (Owner.direction != 1)
		{
			Owner.itemRotation -= 3.14f;
		}

		Owner.heldProj = Projectile.whoAmI;

		Projectile.frameCounter++;

		if (Projectile.frameCounter % 6 == 5)
		{
			Projectile.frame++;

			if (Projectile.frame == 4)
			{
				Shoot();
			}

			if (Projectile.frame == 6)
			{
				//Finished the animation, delete/stop the animation
				Projectile.active = false;
			}
		}

		if (Projectile.frame >= 6)
		{
			Projectile.frame = 0;
			Projectile.frameCounter = 0;
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
		Terraria.Audio.SoundEngine.PlaySound(SoundID.Item5, Projectile.Center);
		//TODO Send a fast projectile from the base
	}
}