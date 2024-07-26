using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Miscellaneous;

internal class QuadBarrelShotgun : VanillaClone
{
	protected override short VanillaItemId => ItemID.QuadBarrelShotgun;

	public override void SetDefaults()
	{
		ItemType = Core.ItemType.Melee;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		Projectile.NewProjectile(source, position.X, position.Y, velocity.X, velocity.Y, type, damage, knockback, player.whoAmI);

		for (int i = 0; i < 7; i++)
		{
			Vector2 adjustedVelocity = velocity;
			float magnitude = adjustedVelocity.Length();
			adjustedVelocity += adjustedVelocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2 * Main.rand.NextFloat()) * Main.rand.NextFloatDirection() * 5f;
			adjustedVelocity = adjustedVelocity.SafeNormalize(Vector2.Zero) * magnitude;
			float useVelX = adjustedVelocity.X;
			float useVelY = adjustedVelocity.Y;
			useVelX += Main.rand.Next(-40, 41) * 0.05f;
			useVelY += Main.rand.Next(-40, 41) * 0.05f;
			Projectile.NewProjectile(source, position.X, position.Y, useVelX, useVelY, type, damage, knockback, player.whoAmI);
		}

		return false;
	}
}
