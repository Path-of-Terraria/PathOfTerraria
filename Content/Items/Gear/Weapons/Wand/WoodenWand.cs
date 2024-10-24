using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Wand;

internal class WoodenWand : Wand
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 4;
		Item.mana = 2;
		Item.useTime = Item.useAnimation = 24;
		Item.shoot = ModContent.ProjectileType<WoodenWandProjectile>();
		Item.shootSpeed = 10;
		Item.UseSound = SoundID.Item7;
	}

	public class WoodenWandProjectile : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.Size = new(16);
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 200;
		}

		public override void AI()
		{
			Projectile.velocity.Y += 0.1f;
			Projectile.rotation += Projectile.velocity.X * 0.05f;

			if (Projectile.timeLeft < 20)
			{
				Projectile.velocity *= 0.95f;
				Projectile.Opacity = Projectile.timeLeft / 20f;
			}

			if (Main.rand.NextBool(30))
			{
				Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, GoreID.TreeLeaf_Normal);
			}
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 20; ++i)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GrassBlades, Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f);
			}
		}
	}
}
