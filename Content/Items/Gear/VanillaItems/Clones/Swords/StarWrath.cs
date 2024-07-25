using PathOfTerraria.Common.Enums;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class StarWrath : VanillaClone
{
	protected override short VanillaItemId => ItemID.StarWrath;

	public override void Defaults()
	{
		ItemType = ItemType.Melee;
		base.Defaults();
	}

	public override void MeleeEffects(Player player, Rectangle box)
	{
		int dust = Dust.NewDust(new Vector2(box.X, box.Y), box.Width, box.Height, DustID.Enchanted_Pink, 0f, 0f, 150, default, 1.2f);
		Main.dust[dust].velocity *= 0.5f;

		if (Main.rand.NextBool(8))
		{
			int otherDust = Gore.NewGore(player.GetSource_ItemUse(Item), new Vector2(box.Center.X, box.Center.Y), default, 16);
			Main.gore[otherDust].velocity *= 0.5f;
			Main.gore[otherDust].velocity += new Vector2(player.direction, 0f);
		}
	}

	public override bool Shoot(Player plr, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		float minimumPassThroughY = Main.MouseWorld.Y;

		if (minimumPassThroughY > plr.Center.Y - 200f)
		{
			minimumPassThroughY = plr.Center.Y - 200f;
		}

		for (int i = 0; i < 3; i++)
		{
			Vector2 pointPoisition = plr.Center + new Vector2(-Main.rand.Next(0, 401) * plr.direction, -600f - 100 * i);
			Vector2 direction = Main.MouseWorld - pointPoisition;

			if (direction.Y < 0f)
			{
				direction.Y *= -1f;
			}

			if (direction.Y < 20f)
			{
				direction.Y = 20f;
			}

			direction = Vector2.Normalize(direction) * Item.shootSpeed;
			velocity.X = direction.X;
			velocity.Y = direction.Y;
			float velX = velocity.X;
			float velY = velocity.Y + Main.rand.Next(-40, 41) * 0.02f;
			Projectile.NewProjectile(source, pointPoisition.X, pointPoisition.Y, velX, velY, type, damage, knockback, plr.whoAmI, 0f, minimumPassThroughY);
		}

		return false;
	}
}
