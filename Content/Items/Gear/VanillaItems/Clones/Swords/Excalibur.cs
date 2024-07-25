using PathOfTerraria.Common.Enums;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class Excalibur : VanillaClone
{
	protected override short VanillaItemId => ItemID.Excalibur;

	public override void Defaults()
	{
		ItemType = ItemType.Melee;
		base.Defaults();
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		float adjustedItemScale2 = player.GetAdjustedItemScale(Item);
		Projectile.NewProjectile(source, player.MountedCenter, new Vector2(player.direction, 0f), type, damage, knockback, player.whoAmI, 
			player.direction * player.gravDir, player.itemAnimationMax, adjustedItemScale2);

		if (Main.netMode != NetmodeID.SinglePlayer)
		{
			NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, player.whoAmI);
		}

		return true;
	}
}
