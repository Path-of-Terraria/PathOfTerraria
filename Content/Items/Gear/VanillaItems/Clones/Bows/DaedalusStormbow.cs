using PathOfTerraria.Core.Items;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Bows;

internal class DaedalusStormbow : VanillaClone
{
	protected override short VanillaItemId => ItemID.DaedalusStormbow;

	public override void SetDefaults()
	{
		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Core.ItemType.Ranged;
	}

	public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		player.itemRotation = (Main.MouseWorld.X - player.Center.X) / 2000f - MathHelper.PiOver2 * player.direction;

		NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, player.whoAmI);
		NetMessage.SendData(MessageID.ShotAnimationAndSound, -1, -1, null, player.whoAmI);

		int projectileCount = 3;

		if (type == ProjectileID.HolyArrow || type == ProjectileID.UnholyArrow || type == ProjectileID.JestersArrow || type == ProjectileID.HellfireArrow)
		{
			if (Main.rand.NextBool(3))
			{
				projectileCount--;
			}
		}
		else if (Main.rand.NextBool(3))
		{
			projectileCount++;
		}

		for (int k = 0; k < projectileCount; k++)
		{
			float baseX = position.X + player.width * 0.5f + Main.rand.Next(201) * -player.direction + (Main.MouseWorld.X - position.X);
			Vector2 basePos = new Vector2(baseX, player.MountedCenter.Y - 600f);
			basePos.X = (basePos.X * 10f + player.Center.X) / 11f + Main.rand.Next(-100, 101);
			basePos.Y -= 150 * k;
			velocity.X = Main.mouseX + Main.screenPosition.X - basePos.X;
			velocity.Y = Main.mouseY + Main.screenPosition.Y - basePos.Y;

			if (velocity.Y < 0f)
			{
				velocity.Y *= -1f;
			}

			if (velocity.Y < 20f)
			{
				velocity.Y = 20f;
			}

			float magnitude = velocity.Length();
			magnitude = Item.shootSpeed / magnitude;
			velocity.X *= magnitude;
			velocity.Y *= magnitude;
			float velX = velocity.X + Main.rand.Next(-40, 41) * 0.03f;
			float velY = velocity.Y + Main.rand.Next(-40, 41) * 0.03f;
			velX *= Main.rand.Next(75, 150) * 0.01f;
			basePos.X += Main.rand.Next(-50, 51);
			int proj = Projectile.NewProjectile(source, basePos.X, basePos.Y, velX, velY, type, damage, knockback, Main.myPlayer);
			Main.projectile[proj].noDropItem = true;
		}

		return false;
	}
}
