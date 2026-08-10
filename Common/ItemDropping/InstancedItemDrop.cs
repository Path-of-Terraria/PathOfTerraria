using Terraria.ID;

namespace PathOfTerraria.Common.ItemDropping;

internal static class InstancedItemDrop
{
	/// <summary>
	/// Drops an item locally for each player accepted by <paramref name="canDrop"/>.
	/// In multiplayer, each accepted player receives a client-only item which other players cannot see or pick up.
	/// </summary>
	public static void DropForEachEligiblePlayer(NPC npc, int itemType, Func<Player, bool> canDrop, int stack = 1,
		bool interactionRequired = true)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient || itemType <= ItemID.None
			|| itemType >= ItemLoader.ItemCount || stack <= 0)
		{
			return;
		}

		bool IsEligible(Player player)
		{
			return player.active
				&& (!interactionRequired || npc.playerInteraction[player.whoAmI])
				&& canDrop(player);
		}

		if (Main.netMode == NetmodeID.SinglePlayer)
		{
			if (IsEligible(Main.LocalPlayer))
			{
				Item.NewItem(npc.GetSource_Death(), npc.Hitbox, itemType, stack);
			}

			return;
		}

		int itemIndex = -1;

		foreach (Player player in Main.ActivePlayers)
		{
			if (!IsEligible(player))
			{
				continue;
			}

			if (itemIndex == -1)
			{
				itemIndex = Item.NewItem(npc.GetSource_Death(), npc.Hitbox, itemType, stack, noBroadcast: true);
				Main.timeItemSlotCannotBeReusedFor[itemIndex] = 54000;
			}

			NetMessage.SendData(MessageID.InstancedItem, player.whoAmI, -1, null, itemIndex);
		}

		if (itemIndex != -1)
		{
			Main.item[itemIndex].active = false;
		}
	}
}
