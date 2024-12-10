using PathOfTerraria.Common.Systems;
using PathOfTerraria.Content.Items.Gear.Weapons.Bow;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Ranged;

internal class BowAnimationProjectile : ModProjectile
{
	public static readonly Dictionary<int, Action<Projectile>> OverridenShootActionsByItemId = [];

	public override string Texture => "Terraria/Images/NPC_0";

	private Player Owner => Main.player[Projectile.owner];

	private ref float AnimationSpeed => ref Projectile.ai[0];
	private ref float Cooldown => ref Projectile.ai[1];
	
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
		// Move bow towards mouse so it looks like it's being held
		Projectile.Center = Owner.Center - new Vector2(0, 8) + Owner.DirectionTo(Main.MouseWorld) * Owner.HeldItem.width * 0.5f;
		Projectile.frameCounter++;
		Owner.itemRotation = Projectile.rotation;
		Owner.heldProj = Projectile.whoAmI;

		if (Owner.direction != 1)
		{
			Owner.itemRotation -= 3.14f;
		}

		if (Projectile.frameCounter % (int)AnimationSpeed == (int)AnimationSpeed - 1)
		{
			if (Projectile.frame < 6)
			{
				Projectile.frame++;
			}

			if (Projectile.frame == 4)
			{
				if (!OverridenShootActionsByItemId.TryGetValue(Owner.HeldItem.type, out Action<Projectile> action))
				{
					Shoot();
				}
				else
				{
					action(Projectile);
				}
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
		if (Owner.whoAmI != Main.myPlayer || Owner.HeldItem.ModItem is not Bow bow)
		{
			return;
		}

		Terraria.Audio.SoundEngine.PlaySound(SoundID.Item5, Projectile.Center);
		Owner.PickAmmo(Owner.HeldItem, out int type, out float speed, out int damage, out float kB, out int ammoUsed);
		Owner.GetModPlayer<AltUsePlayer>().SetAltCooldown((int)(Cooldown * 60f));

		damage = (int)(damage * 3f);
		Vector2 vel = Projectile.DirectionTo(Main.MouseWorld) * speed * 1.5f;
		IEntitySource src = Owner.GetSource_ItemUse_WithPotentialAmmo(Owner.HeldItem, ammoUsed);
		Projectile.NewProjectile(src, Projectile.Center, vel, ProjectileID.WoodenArrowFriendly, damage, kB, Owner.whoAmI);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		// Manually draw the projectile so it aligns properly when rotated
		Texture2D tex = Bow.BowProjectileSpritesById[Owner.HeldItem.type].Value;
		int frameHeight = tex.Height / Main.projFrames[Type];
		var src = new Rectangle(0, frameHeight * Projectile.frame, tex.Width, frameHeight);
		Color color = Lighting.GetColor(Projectile.Center.ToTileCoordinates());
		Vector2 position = Projectile.Center - Main.screenPosition - (Owner.HeldItem.ModItem.HoldoutOffset() ?? Vector2.Zero);

		Main.spriteBatch.Draw(tex, position, src, color, Projectile.rotation, src.Size() / 2f, 1f, SpriteEffects.None, 0);
		return false;
	}
}