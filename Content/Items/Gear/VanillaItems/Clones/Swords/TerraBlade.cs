using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class TerraBlade : VanillaClone
{
	protected override short VanillaItemId => ItemID.TerraBlade;

	public override void Defaults()
	{
		ItemType = Core.ItemType.Melee;
		base.Defaults();
	}

	public override bool Shoot(Player plr, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		float scale = plr.GetAdjustedItemScale(Item);
		int dir = plr.direction;
		Projectile.NewProjectile(source, position, new Vector2(dir, 0f), ProjectileID.TerraBlade2, damage, knockback, plr.whoAmI, dir * plr.gravDir, plr.itemAnimationMax, scale);
		Projectile.NewProjectile(source, position, velocity * 5f, type, damage, knockback, plr.whoAmI, dir * plr.gravDir, 18f, scale);
		NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, plr.whoAmI);

		return true;
	}
}
