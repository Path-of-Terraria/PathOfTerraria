using PathOfTerraria.Common.Enums;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Launchers;

internal class Celebration : VanillaClone
{
	protected override short VanillaItemId => ItemID.FireworksLauncher;

	public override void Defaults()
	{
		ItemType = ItemType.Melee;
		base.Defaults();
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		for (int i = 0; i < 2; i++)
		{
			Vector2 useVel = velocity;
			useVel.X += Main.rand.Next(-40, 41) * 0.05f;
			useVel.Y += Main.rand.Next(-40, 41) * 0.05f;
			Vector2 adjPos = position + Vector2.Normalize(useVel.RotatedBy(-(float)Math.PI / 2f * player.direction)) * 6f;
			Projectile.NewProjectile(source, adjPos.X, adjPos.Y, useVel.X, useVel.Y, 167 + Main.rand.Next(4), damage, knockback, player.whoAmI, 0f, 1f);
		}

		return false;
	}
}
