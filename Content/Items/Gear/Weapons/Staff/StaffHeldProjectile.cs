using Terraria.GameContent;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Staff;

/// <summary>
/// Mimics a held staff so it looks like the player is dynamically using the item. Purely visual.
/// </summary>
internal abstract class StaffHeldProjectile : ModProjectile
{
	protected Player Owner => Main.player[Projectile.owner];

	public override void SetDefaults()
	{
		Projectile.Size = new(1);
		Projectile.friendly  = true;
		Projectile.hostile = false;
		Projectile.timeLeft = 3000;
	}

	public override void AI()
	{
		Owner.heldProj = Projectile.whoAmI;

		if (!Owner.channel)
		{
			Projectile.Kill();
			return;
		}

		Projectile.Center = Owner.Center + Owner.RotatedRelativePoint(Vector2.Zero);

		if (Main.myPlayer == Projectile.owner)
		{
			Projectile.rotation = Projectile.AngleTo(Main.MouseWorld) + MathHelper.PiOver4;

			Owner.direction = Main.MouseWorld.X <= Owner.Center.X ? -1 : 1;
			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, Projectile.rotation - MathHelper.PiOver4 - MathHelper.PiOver2);
			Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter, Projectile.rotation - MathHelper.PiOver4 - MathHelper.PiOver2);
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;

		Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, new Vector2(0, tex.Height), 1f, SpriteEffects.None, 0);
		return false;
	}
}
