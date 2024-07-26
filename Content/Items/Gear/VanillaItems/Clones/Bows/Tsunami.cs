using PathOfTerraria.Core.Items;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Bows;

internal class Tsunami : VanillaClone
{
	protected override short VanillaItemId => ItemID.Tsunami;

	public override void SetDefaults()
	{
		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Core.ItemType.Ranged;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 vel, int type, int damage, float knockback)
	{
		const int ShotCount = 5;

		float num128 = (float)Math.PI / 10f;
		Vector2 baseOffset = Vector2.Normalize(vel) * 40f;
		bool inTiles = Collision.CanHit(position, 0, 0, position + baseOffset, 0, 0);

		for (int i = 0; i < ShotCount; i++)
		{
			float rotation = i - (ShotCount - 1f) / 2f;
			Vector2 posOffset = baseOffset.RotatedBy(num128 * rotation);

			if (!inTiles)
			{
				posOffset -= baseOffset;
			}

			int proj = Projectile.NewProjectile(source, position.X + posOffset.X, position.Y + posOffset.Y, vel.X, vel.Y, type, damage, knockback, Main.myPlayer);
			Main.projectile[proj].noDropItem = true;
		}

		return false;
	}
}
