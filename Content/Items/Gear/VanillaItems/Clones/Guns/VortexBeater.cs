using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Miscellaneous;

internal class VortexBeater : VanillaClone
{
	protected override short VanillaItemId => ItemID.VortexBeater;

	public override void SetDefaults()
	{
		ItemType = Core.ItemType.Melee;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		Projectile.NewProjectile(source, position.X, position.Y, velocity.X, velocity.Y, ProjectileID.VortexBeater, damage, knockback, player.whoAmI, 5 * Main.rand.Next(0, 20));
		return false;
	}
}
