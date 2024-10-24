using PathOfTerraria.Core.Items;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Wand;

internal class GemWand : Wand
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.MinDropItemLevel = 17;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 22;
		Item.mana = 4;
		Item.useTime = Item.useAnimation = 26;
		Item.shoot = ModContent.ProjectileType<GemWandProjectile>();
		Item.shootSpeed = 12;
		Item.UseSound = SoundID.Item7;
	}

	public class GemWandProjectile : ModProjectile
	{
		public override string Texture => "Terraria/Images/NPC_0";

		private int ItemId
		{
			get => (int)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override void SetDefaults()
		{
			Projectile.Size = new(20);
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 250;
		}

		public override void AI()
		{
			if (ItemId == ItemID.None)
			{
				ItemId = Main.rand.Next([ItemID.Emerald, ItemID.Ruby, ItemID.Diamond, ItemID.Sapphire, ItemID.Amethyst, ItemID.Topaz]);

				Projectile.netUpdate = true;
			}

			Projectile.velocity.Y += 0.05f;
			Projectile.rotation += Projectile.velocity.X * 0.04f;

			if (Projectile.timeLeft < 20)
			{
				Projectile.Opacity = Projectile.timeLeft / 20f;
			}

			if (Main.rand.NextBool(25))
			{
				Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, GetDust(), Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f);
			}
		}

		private short GetDust()
		{
			return ItemId switch
			{
				ItemID.Emerald => DustID.GemEmerald,
				ItemID.Ruby => DustID.GemRuby,
				ItemID.Amethyst => DustID.GemAmethyst,
				ItemID.Sapphire => DustID.GemSapphire,
				ItemID.Topaz => DustID.GemTopaz,
				_ => DustID.GemDiamond
			};
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 14; ++i)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, GetDust(), Projectile.velocity.X * 0.25f, Projectile.velocity.Y * 0.25f);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (ItemId == ItemID.None)
			{
				return false;
			}

			Main.instance.LoadItem(ItemId);
			Texture2D tex = TextureAssets.Item[ItemId].Value;
			Color color = Color.Lerp(lightColor, Color.White, 0.2f) * Projectile.Opacity;
			
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, tex.Size() / 2f, 1f, SpriteEffects.None, 0);
			return false;
		}
	}
}
