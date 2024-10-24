using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Wand;

internal class MahoganyWand : Wand
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.MinDropItemLevel = 5;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 8;
		Item.mana = 3;
		Item.useTime = Item.useAnimation = 24;
		Item.shoot = ModContent.ProjectileType<MahoganyWandProjectile>();
		Item.shootSpeed = 10;
		Item.UseSound = SoundID.Item7;
	}

	public class MahoganyWandProjectile : ModProjectile
	{
		public override void SetDefaults()
		{
			Projectile.Size = new(18);
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 220;
		}

		public override void AI()
		{
			Projectile.velocity.Y += 0.08f;
			Projectile.rotation += Projectile.velocity.X * 0.05f;

			if (Projectile.timeLeft < 20)
			{
				Projectile.velocity *= 0.95f;
				Projectile.Opacity = Projectile.timeLeft / 20f;
			}

			if (Main.rand.NextBool(32))
			{
				Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, GoreID.TreeLeaf_Jungle);
			}
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 20; ++i)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.JungleGrass, Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f);
			}
		}
	}
}
