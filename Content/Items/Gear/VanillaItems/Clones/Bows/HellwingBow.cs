using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Bows;

internal class HellwingBow : VanillaClone
{
	protected override short VanillaItemId => ItemID.HellwingBow;

	public override void Defaults()
	{
		ItemType = Core.ItemType.Ranged;
		base.Defaults();
	}

	public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
	{
		if (type == ProjectileID.WoodenArrowFriendly)
		{
			type = ProjectileID.Hellwing;
		}
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		Vector2 baseVelocity = velocity;
		float magnitude = baseVelocity.Length();
		baseVelocity.X += Main.rand.Next(-100, 101) * 0.01f * magnitude * 0.15f;
		baseVelocity.Y += Main.rand.Next(-100, 101) * 0.01f * magnitude * 0.15f;
		float velX = velocity.X + Main.rand.Next(-40, 41) * 0.03f;
		float velY = velocity.Y + Main.rand.Next(-40, 41) * 0.03f;
		baseVelocity = Vector2.Normalize(baseVelocity) * magnitude;
		velX *= Main.rand.Next(50, 150) * 0.01f;
		velY *= Main.rand.Next(50, 150) * 0.01f;
		Vector2 finalVel = Vector2.Normalize(new Vector2(velX + Main.rand.Next(-100, 101) * 0.025f, velY + Main.rand.Next(-100, 101) * 0.025f)) * magnitude;
		Projectile.NewProjectile(source, position.X, position.Y, finalVel.X, finalVel.Y, type, damage, knockback, player.whoAmI, baseVelocity.X, baseVelocity.Y);

		return false;
	}
}
