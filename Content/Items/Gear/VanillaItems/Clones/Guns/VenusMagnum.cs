using PathOfTerraria.Common.Enums;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Guns;

internal class VenusMagnum : VanillaClone
{
	protected override short VanillaItemId => ItemID.VenusMagnum;

	public override void Defaults()
	{
		ItemType = ItemType.Melee;
		base.Defaults();
	}

	public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
	{
		if (type == ProjectileID.Bullet)
		{
			type = ProjectileID.BulletHighVelocity;
		}
	}
}
