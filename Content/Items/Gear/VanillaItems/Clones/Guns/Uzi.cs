using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Miscellaneous;

internal class Uzi : VanillaClone
{
	protected override short VanillaItemId => ItemID.Uzi;

	public override void SetDefaults()
	{
		ItemType = Core.ItemType.Melee;
	}

	public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
	{
		if (type == ProjectileID.Bullet)
		{
			type = ProjectileID.BulletHighVelocity;
		}
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		velocity.X += Main.rand.Next(-30, 31) * 0.03f;
		velocity.Y += Main.rand.Next(-30, 31) * 0.03f;
		Projectile.NewProjectile(source, position.X, position.Y, velocity.X, velocity.Y, type, damage, knockback, player.whoAmI);

		return false;
	}
}
