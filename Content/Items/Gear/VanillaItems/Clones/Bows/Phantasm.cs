using PathOfTerraria.Core.Items;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Bows;

internal class Phantasm : VanillaClone
{
	protected override short VanillaItemId => ItemID.Phantasm;

	public override void SetDefaults()
	{
		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Core.ItemType.Ranged;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		Projectile.NewProjectile(source, position.X, position.Y, velocity.X, velocity.Y, ProjectileID.Phantasm, damage, knockback, player.whoAmI);
		return false;
	}

	public override bool CanConsumeAmmo(Item ammo, Player player)
	{
		return !Main.rand.NextBool(3);
	}
}
