using PathOfTerraria.Common.Enums;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Bows;

internal class ShadowflameBow : VanillaClone
{
	protected override short VanillaItemId => ItemID.ShadowFlameBow;

	public override void Defaults()
	{
		ItemType = ItemType.Ranged;
		base.Defaults();
	}

	public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
	{
		type = ProjectileID.ShadowFlameArrow;
	}
}
