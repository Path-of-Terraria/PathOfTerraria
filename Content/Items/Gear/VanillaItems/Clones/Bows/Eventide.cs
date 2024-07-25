using PathOfTerraria.Common.Enums;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Bows;

internal class Eventide : VanillaClone
{
	protected override short VanillaItemId => ItemID.FairyQueenRangedItem;

	public override void Defaults()
	{
		ItemType = ItemType.Ranged;
		base.Defaults();
	}

	public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
	{
		if (type == ProjectileID.WoodenArrowFriendly)
		{
			type = ProjectileID.FairyQueenRangedItemShot;
		}
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		const float PiOver10 = (float)Math.PI / 10f;

		Vector2 baseVelocity = Vector2.Normalize(velocity) * 40;
		bool inTiles = Collision.CanHit(position, 0, 0, position + baseVelocity, 0, 0);
		int timer = (player.itemAnimationMax - player.itemAnimation) / 2;
		int baseRotation = timer;

		if (player.direction == 1)
		{
			baseRotation = 4 - timer;
		}

		float rotation = baseRotation - (5 - 1f) / 2f;
		Vector2 vector16 = baseVelocity.RotatedBy(PiOver10 * rotation);

		if (!inTiles)
		{
			vector16 -= baseVelocity;
		}

		Vector2 origin = position + vector16;
		Vector2 velocityMod = origin.DirectionTo(Main.MouseWorld).SafeNormalize(-Vector2.UnitY);
		Vector2 adjustedTarget = player.Center.DirectionTo(player.Center + baseVelocity).SafeNormalize(-Vector2.UnitY);
		float lerpValue = Utils.GetLerpValue(100f, 40f, Main.MouseWorld.Distance(player.Center), clamped: true);

		if (lerpValue > 0f)
		{
			velocityMod = Vector2.Lerp(velocityMod, adjustedTarget, lerpValue).SafeNormalize(velocity.SafeNormalize(-Vector2.UnitY));
		}

		Vector2 finalVelocity = velocityMod * Item.shootSpeed;

		if (timer == 2)
		{
			type = ProjectileID.FairyQueenRangedItemShot;
			damage *= 2;
		}

		if (type == ProjectileID.FairyQueenRangedItemShot)
		{
			float ai3 = player.miscCounterNormalized * 12f % 1f;
			finalVelocity = finalVelocity.SafeNormalize(Vector2.Zero) * (Item.shootSpeed * 2f);
			Projectile.NewProjectile(source, origin, finalVelocity, type, damage, knockback, player.whoAmI, 0f, ai3);
		}
		else
		{
			int proj = Projectile.NewProjectile(source, origin, finalVelocity, type, damage, knockback, player.whoAmI);
			Main.projectile[proj].noDropItem = true;
		}

		return false;
	}
}
