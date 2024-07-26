using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Miscellaneous;

internal class SniperRifle : VanillaClone
{
	protected override short VanillaItemId => ItemID.SniperRifle;

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

	public override void HoldItem(Player player)
	{
		base.HoldItem(player);

		player.scope = true;
	}
}
