using PathOfTerraria.Common.Enums;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Miscellaneous;

internal class VampireKnives : VanillaClone
{
	protected override short VanillaItemId => ItemID.VampireKnives;

	public override void Defaults()
	{
		ItemType = ItemType.Ranged;
		base.Defaults();
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		// This code is adapted from vanilla's Player.ItemCheck_Shoot method.
		// It's a little messy, but this is hard to read and I want to make sure it functions 1:1 or as close as I can get to vanilla.
		int numShots = 4;

		if (Main.rand.NextBool())
		{
			numShots++;
		}

		for (int i = 2; i <= 4; ++i)
		{
			if (Main.rand.NextBool(i * i))
			{
				numShots++;
			}
		}

		Vector2 pointPoisition = player.RotatedRelativePoint(player.MountedCenter);
		float baseTargetX = Main.mouseX + Main.screenPosition.X - pointPoisition.X;
		float baseTargetY = Main.mouseY + Main.screenPosition.Y - pointPoisition.Y;

		for (int i = 0; i < numShots; i++)
		{
			float targetX = baseTargetX;
			float targetY = baseTargetY;
			float rotationFactor = 1.5f * i;
			targetX += Main.rand.Next(-35, 36) * rotationFactor;
			targetY += Main.rand.Next(-35, 36) * rotationFactor;
			float targetDistance = (float)Math.Sqrt(targetX * targetX + targetY * targetY);
			targetDistance = Item.shootSpeed / targetDistance;
			targetX *= targetDistance;
			targetY *= targetDistance;

			Projectile.NewProjectile(source, pointPoisition.X, pointPoisition.Y, targetX, targetY, type, damage, knockback, Main.myPlayer);
		}

		return true;
	}
}
