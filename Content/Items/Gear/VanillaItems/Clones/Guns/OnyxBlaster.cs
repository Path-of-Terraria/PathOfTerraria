using PathOfTerraria.Common.Enums;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Guns;

internal class OnyxBlaster : VanillaClone
{
	protected override short VanillaItemId => ItemID.OnyxBlaster;

	public override void Defaults()
	{
		ItemType = ItemType.Melee;
		base.Defaults();
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 pos, Vector2 vel, int type, int damage, float knockback)
	{
		const float OneFourthPi = MathHelper.PiOver4;

		for (int num82 = 0; num82 < 2; num82++)
		{
			Vector2 firstVel = vel + vel.SafeNormalize(Vector2.Zero).RotatedBy(OneFourthPi * Main.rand.NextFloat(0.5f, 1f)) * Main.rand.NextFloatDirection() * 2f;
			Projectile.NewProjectile(source, pos, firstVel, type, damage, knockback, player.whoAmI);

			Vector2 secondVel = vel + vel.SafeNormalize(Vector2.Zero).RotatedBy(-OneFourthPi * Main.rand.NextFloat(0.5f, 1f)) * Main.rand.NextFloatDirection() * 2f;
			Projectile.NewProjectile(source, pos, secondVel, type, damage, knockback, player.whoAmI);
		}

		Vector2 onyxVel = vel.SafeNormalize(Vector2.UnitX * player.direction) * (Item.shootSpeed * 1.3f);
		Projectile.NewProjectile(source, pos, onyxVel, ProjectileID.BlackBolt, damage * 2, knockback, Main.myPlayer);
		return false;
	}
}
