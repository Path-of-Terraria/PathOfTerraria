using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

internal class Umbrella : VanillaClone
{
	protected override short VanillaItemId => ItemID.Umbrella;

	public override void SetDefaults()
	{
		ItemType = Core.ItemType.Melee;
	}

	// Umbrella functionality is super hardcoded because vanilla is fun
	// All methods below this replace all functionality
	public override void HoldStyle(Player player, Rectangle heldItemFrame)
	{
		player.itemRotation = 0;
		player.itemLocation.X = (int)(player.Center.X - 16 * player.direction);
		player.itemLocation.Y += 6;
		player.fallStart = (int)(player.position.Y / 16f);

		if (player.gravDir == -1f)
		{
			player.itemRotation = 0f - player.itemRotation;
			player.itemLocation.Y = player.position.Y + player.height + (player.position.Y - player.itemLocation.Y);

			if (player.velocity.Y < -2f && !player.controlDown)
			{
				player.velocity.Y = -2f;
			}
		}
		else if (player.velocity.Y > 2f && !player.controlDown)
		{
			player.velocity.Y = 2f;
		}
	}

	public override void UseStyle(Player player, Rectangle heldItemFrame)
	{
		player.itemRotation = player.direction == -1 ? -MathHelper.PiOver2 : MathHelper.PiOver2;
		player.itemLocation.Y -= 22;
		heldItemFrame.Height += 14;
		heldItemFrame.Width -= 10;

		if (player.direction == -1)
		{
			heldItemFrame.X += 10;
		}
	}

	public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
	{
		if (player.mount.Active)
		{
			hitbox.Y += player.mount.YOffset;
		}

		hitbox.Y += 8;
		hitbox.Height += 14;
	}
}
