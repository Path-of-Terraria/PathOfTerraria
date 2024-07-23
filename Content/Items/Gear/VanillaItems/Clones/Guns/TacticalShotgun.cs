using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Miscellaneous;

internal class TacticalShotgun : VanillaClone
{
	protected override short VanillaItemId => ItemID.TacticalShotgun;

	public override void Defaults()
	{
		ItemType = Core.ItemType.Melee;
		base.Defaults();
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		for (int i = 0; i < 6; i++)
		{
			Vector2 useVel = velocity;
			useVel.X += Main.rand.Next(-40, 41) * 0.05f;
			useVel.Y += Main.rand.Next(-40, 41) * 0.05f;
			Projectile.NewProjectile(source, position.X, position.Y, useVel.X, useVel.Y, type, damage, knockback, player.whoAmI);
		}

		return false;
	}
}
