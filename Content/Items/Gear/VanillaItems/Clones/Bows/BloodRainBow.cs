using System.Drawing;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Bows;

internal class BloodRainBow : VanillaClone
{
	protected override short VanillaItemId => ItemID.BloodRainBow;

	public override void SetDefaults()
	{
		ItemType = Core.ItemType.Ranged;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		player.itemRotation = (Main.MouseWorld.X - player.Center.X) / 2000f - MathHelper.PiOver2 * player.direction;

		NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, player.whoAmI);
		NetMessage.SendData(MessageID.ShotAnimationAndSound, -1, -1, null, player.whoAmI);

		int projCount = Main.rand.Next(1, 3);

		if (Main.rand.NextBool(3))
		{
			projCount++;
		}

		for (int l = 0; l < projCount; l++)
		{
			float baseX = position.X + player.width * 0.5f + Main.rand.Next(61) * -player.direction + (Main.MouseWorld.X - position.X);
			var pos = new Vector2(baseX, player.MountedCenter.Y - 600f);
			pos.X = (pos.X * 10f + player.Center.X) / 11f + Main.rand.Next(-30, 31);
			pos.Y -= 150f * Main.rand.NextFloat();

			Vector2 useVel = velocity;
			useVel.X = Main.mouseX + Main.screenPosition.X - pos.X;
			useVel.Y = Main.mouseY + Main.screenPosition.Y - pos.Y;

			if (useVel.Y < 0f)
			{
				useVel.Y *= -1f;
			}

			if (useVel.Y < 20f)
			{
				useVel.Y = 20f;
			}

			float magnitude = useVel.Length();
			magnitude = Item.shootSpeed / magnitude;
			useVel.X *= magnitude;
			useVel.Y *= magnitude;
			float xVel = useVel.X + Main.rand.Next(-20, 21) * 0.03f;
			float yVel = useVel.Y + Main.rand.Next(-40, 41) * 0.03f;
			xVel *= Main.rand.Next(55, 80) * 0.01f;
			pos.X += Main.rand.Next(-50, 51);

			int proj = Projectile.NewProjectile(source, pos.X, pos.Y, xVel, yVel, ProjectileID.BloodArrow, damage, knockback, Main.myPlayer);
			Main.projectile[proj].noDropItem = true;
		}

		return false;
	}
}
