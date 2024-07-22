using System.Reflection;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class Frostbrand : VanillaClone
{
	protected override short VanillaItemId => ItemID.Frostbrand;

	public override void Defaults()
	{
		ItemType = Core.ItemType.Melee;
		base.Defaults();
		Item.shootsEveryUse = true;
	}

	// The vanilla weapon shoots projectiles significantly less often than this does.
	// The code I've seen is hardcoded, per usual, and there's no simple way to replicate.
	// Will leave for later, if ever.

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		int proj = Projectile.NewProjectile(source, position.X, position.Y, velocity.X, velocity.Y, type, damage, knockback, player.whoAmI);
		Main.projectile[proj].DamageType = DamageClass.Melee;
		return false;
	}

	public override void MeleeEffects(Player player, Rectangle hitbox)
	{
		Rectangle rect = SwordCommon.GetItemRectangle(player, Item);
		int dust = Dust.NewDust(new Vector2(rect.X, rect.Y), rect.Width, rect.Height, DustID.IceRod, player.velocity.X * 0.2f + player.direction * 3, 
			player.velocity.Y * 0.2f, 90, default, 1.5f);
		Main.dust[dust].noGravity = true;
		Main.dust[dust].velocity *= 0.2f;
	}
}