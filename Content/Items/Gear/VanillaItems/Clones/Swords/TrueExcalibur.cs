using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class TrueExcalibur : VanillaClone
{
	protected override short VanillaItemId => ItemID.TrueExcalibur;

	public override void Defaults()
	{
		ItemType = Core.ItemType.Melee;
		base.Defaults();
	}

	public override bool Shoot(Player plr, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		float scale = plr.GetAdjustedItemScale(Item);
		int dir = plr.direction;
		Projectile.NewProjectile(source, position, new Vector2(dir, 0f), type, damage, knockback, plr.whoAmI, dir * plr.gravDir, plr.itemAnimationMax, scale);
		Projectile.NewProjectile(source, position, new Vector2(dir, 0f), ProjectileID.Excalibur, 0, knockback, plr.whoAmI, dir * plr.gravDir, plr.itemAnimationMax, scale);

		if (Main.netMode != NetmodeID.SinglePlayer)
		{
			NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, plr.whoAmI);
		}

		return true;
	}
}
