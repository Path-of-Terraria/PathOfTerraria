using Terraria.Graphics.Capture;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.VanillaItems.Clones.Swords;

/// <summary>
/// Contains a bunch of ripped-from-source methods for vanilla parity.
/// These are usually private, so copy-pasting is easier than using reflection and provide the same benefit.
/// </summary>
internal static class SwordCommon
{
	public static Rectangle GetItemRectangle(Player player, Item item)
	{
		var itemRectangle = new Rectangle((int)player.itemLocation.X, (int)player.itemLocation.Y, 32, 32);

		if (!Main.dedServ)
		{
			int itemWidth = player.HeldItem.width;
			int itemHeight = player.HeldItem.height;
			itemRectangle = new Rectangle((int)player.itemLocation.X, (int)player.itemLocation.Y, itemWidth, itemHeight);
		}

		float adjustedItemScale = player.GetAdjustedItemScale(item);
		itemRectangle.Width = (int)(itemRectangle.Width * adjustedItemScale);
		itemRectangle.Height = (int)(itemRectangle.Height * adjustedItemScale);

		if (player.direction == -1)
		{
			itemRectangle.X -= itemRectangle.Width;
		}

		if (player.gravDir == 1f)
		{
			itemRectangle.Y -= itemRectangle.Height;
		}

		if (item.useStyle == ItemUseStyleID.Swing)
		{
			if (player.itemAnimation < player.itemAnimationMax * 0.333)
			{
				if (player.direction == -1)
				{
					itemRectangle.X -= (int)(itemRectangle.Width * 1.4 - itemRectangle.Width);
				}

				itemRectangle.Width = (int)(itemRectangle.Width * 1.4);
				itemRectangle.Y += (int)(itemRectangle.Height * 0.5 * player.gravDir);
				itemRectangle.Height = (int)(itemRectangle.Height * 1.1);
			}
			else if (!(player.itemAnimation < player.itemAnimationMax * 0.666))
			{
				if (player.direction == 1)
				{
					itemRectangle.X -= (int)(itemRectangle.Width * 1.2);
				}

				itemRectangle.Width *= 2;
				itemRectangle.Y -= (int)((itemRectangle.Height * 1.4 - itemRectangle.Height) * player.gravDir);
				itemRectangle.Height = (int)(itemRectangle.Height * 1.4);
			}
		}

		bool dontAttack = false;
		ItemLoader.UseItemHitbox(item, player, ref itemRectangle, ref dontAttack);
		return itemRectangle;
	}

	public static void GetPointOnSwungItemPath(Player player, float spriteWidth, float spriteHeight, float normalizedPointOnPath, float itemScale,
	out Vector2 location, out Vector2 outwardDirection)
	{
		float spriteLength = (float)Math.Sqrt(spriteWidth * spriteWidth + spriteHeight * spriteHeight);
		float baseRotation = (player.direction == 1).ToInt() * ((float)Math.PI / 2f);

		if (player.gravDir == -1f)
		{
			baseRotation += (float)Math.PI / 2f * player.direction;
		}

		outwardDirection = player.itemRotation.ToRotationVector2().RotatedBy(3.926991f + baseRotation);
		location = player.RotatedRelativePoint(player.itemLocation + outwardDirection * spriteLength * normalizedPointOnPath * itemScale);
	}

	public static bool GetGeneralCheck(Player player)
	{
		bool flag = player.selectedItem != 58 && player.controlUseTile && Main.myPlayer == player.whoAmI && !player.tileInteractionHappened &&
			player.releaseUseItem && !player.controlUseItem && !player.mouseInterface && !CaptureManager.Instance.Active && !Main.HoveringOverAnNPC && !Main.SmartInteractShowingGenuine;

		// Skip this check because it's niche and annoying to recreate; can do in the future if needed.
		//if (flag2 && player.altFunctionUse == 0)
		//{
		//	for (int i = 0; i < _projectilesToInteractWith.Count; i++)
		//	{
		//		Projectile projectile = Main.projectile[_projectilesToInteractWith[i]];

		//		if (projectile.Hitbox.Contains(Main.MouseWorld.ToPoint()) || Main.SmartInteractProj == projectile.whoAmI)
		//		{
		//			flag = false;
		//			break;
		//		}
		//	}
		//}

		return flag;
	}

	public static bool GetZenithTarget(Player player, Vector2 searchCenter, float maxDistance, out int npcTargetIndex)
	{
		npcTargetIndex = 0;
		int? index = null;
		float currentDistance = maxDistance;

		for (int i = 0; i < 200; i++)
		{
			NPC nPC = Main.npc[i];
			if (nPC.CanBeChasedBy(player))
			{
				float num3 = searchCenter.Distance(nPC.Center);
				if (!(currentDistance <= num3))
				{
					index = i;
					currentDistance = num3;
				}
			}
		}

		if (!index.HasValue)
		{
			return false;
		}

		npcTargetIndex = index.Value;
		return true;
	}
}
