using PathOfTerraria.Common.Enums;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Bows;

internal class PhantomPhoenix : VanillaClone
{
	protected override short VanillaItemId => ItemID.DD2PhoenixBow;

	public override void Defaults()
	{
		ItemType = ItemType.Ranged;
		base.Defaults();
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		Projectile.NewProjectile(source, position.X, position.Y, velocity.X, velocity.Y, ProjectileID.DD2PhoenixBow, damage, knockback, player.whoAmI);
		return false;
	}
}
