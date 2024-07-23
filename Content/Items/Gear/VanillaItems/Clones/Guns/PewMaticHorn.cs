using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Miscellaneous;

internal class PewMaticHorn : VanillaClone
{
	protected override short VanillaItemId => ItemID.PewMaticHorn;

	public override void Defaults()
	{
		ItemType = Core.ItemType.Melee;
		base.Defaults();
	}

	public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
	{
		type = ProjectileID.PewMaticHornShot;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		velocity.X += Main.rand.Next(-15, 16) * 0.075f;
		velocity.Y += Main.rand.Next(-15, 16) * 0.075f;
		int frame = Main.rand.Next(Main.projFrames[Item.shoot]);
		Projectile.NewProjectile(source, position.X, position.Y, velocity.X, velocity.Y, type, damage, knockback, player.whoAmI, 0f, frame);

		return false;
	}

	public override Vector2? HoldoutOffset()
	{
		return new Vector2(2, 0);
	}
}
