using PathOfTerraria.Core.Items;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Bows;

internal class ChlorophyteShotbow : VanillaClone
{
	protected override short VanillaItemId => ItemID.ChlorophyteShotbow;

	public override void SetDefaults()
	{
		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Core.ItemType.Ranged;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		int projCount = 2;

		if (Main.rand.NextBool(3))
		{
			projCount++;
		}

		for (int i = 0; i < projCount; i++)
		{
			float velX = velocity.X;
			float velY = velocity.Y;

			for (int j = -1; j < i; ++j)
			{
				velX += Main.rand.Next(-35, 36) * 0.04f;
				velY += Main.rand.Next(-35, 36) * 0.04f;
			}

			int proj = Projectile.NewProjectile(source, position.X, position.Y, velX, velY, type, damage, knockback, player.whoAmI);
			Main.projectile[proj].noDropItem = true;
		}

		return false;
	}
}
