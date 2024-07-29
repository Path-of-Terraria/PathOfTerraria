using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class TheHorsemansBlade : VanillaClone
{
	protected override short VanillaItemId => ItemID.TheHorsemansBlade;

	public override void Defaults()
	{
		ItemType = Core.ItemType.Melee;
		base.Defaults();
	}

	public override bool Shoot(Player plr, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		float scale = plr.GetAdjustedItemScale(Item);
		Projectile.NewProjectile(source, position, new Vector2(plr.direction, 0f), type, damage, knockback, plr.whoAmI, plr.direction * plr.gravDir, plr.itemAnimationMax, scale);
		NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, plr.whoAmI);

		return true;
	}
}
