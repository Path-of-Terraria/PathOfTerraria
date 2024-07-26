using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Miscellaneous;

internal class Boomstick : VanillaClone
{
	protected override short VanillaItemId => ItemID.Boomstick;

	public override void SetDefaults()
	{
		ItemType = Core.ItemType.Melee;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		int projCount = Main.rand.Next(3, 5);

		for (int i = 0; i < projCount; i++)
		{
			Vector2 useVel = velocity;
			useVel.X += Main.rand.Next(-35, 36) * 0.04f;
			useVel.Y += Main.rand.Next(-35, 36) * 0.04f;
			Projectile.NewProjectile(source, position.X, position.Y, useVel.X, useVel.Y, type, damage, knockback, player.whoAmI);
		}

		return false;
	}
}
