using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Miscellaneous;

internal class Xenopopper : VanillaClone
{
	protected override short VanillaItemId => ItemID.Xenopopper;

	public override void Defaults()
	{
		ItemType = Core.ItemType.Melee;
		base.Defaults();
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		Vector2 tipOfGun = Vector2.Normalize(velocity) * 40f * Item.scale;

		if (Collision.CanHit(position, 0, 0, position + tipOfGun, 0, 0))
		{
			position += tipOfGun;
		}

		const float TwoThirdsPi = (float)Math.PI * 2f / 3f;

		float ai = velocity.ToRotation();
		int shotCount = Main.rand.Next(4, 5);

		if (Main.rand.NextBool(4))
		{
			shotCount++;
		}

		for (int i = 0; i < shotCount; i++)
		{
			float speedMod = (float)Main.rand.NextDouble() * 0.2f + 0.05f;
			Vector2 useVel = velocity.RotatedBy(TwoThirdsPi * (float)Main.rand.NextDouble() - TwoThirdsPi / 2f) * speedMod;
			int num108 = Projectile.NewProjectile(source, position.X, position.Y, useVel.X, useVel.Y, ProjectileID.Xenopopper, damage, knockback, player.whoAmI, ai);
			Main.projectile[num108].localAI[0] = type;
			Main.projectile[num108].localAI[1] = Item.shootSpeed;
		}

		return false;
	}
}
