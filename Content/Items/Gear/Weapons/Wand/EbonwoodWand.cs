using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Wand;

internal class EbonwoodWand : Wand
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.MinDropItemLevel = 12;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 20;
		Item.mana = 5;
		Item.useTime = Item.useAnimation = 40;
		Item.shoot = ModContent.ProjectileType<EbonwoodWandProjectile>();
		Item.shootSpeed = 10;
		Item.UseSound = SoundID.Item7;
	}

	public class EbonwoodWandProjectile : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.Size = new(20);
			Projectile.friendly = true;
			Projectile.penetrate = 2;
			Projectile.timeLeft = 250;
		}

		public override void AI()
		{
			Projectile.velocity.Y += 0.1f;
			Projectile.rotation += Projectile.velocity.X * 0.04f;

			if (Projectile.timeLeft < 20)
			{
				Projectile.velocity *= 0.95f;
				Projectile.Opacity = Projectile.timeLeft / 20f;
			}

			if (Main.rand.NextBool(32))
			{
				Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, GoreID.TreeLeaf_Corruption);
			}
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 20; ++i)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Corruption, Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f);
			}
		}
	}
}
