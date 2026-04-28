using PathOfTerraria.Common.Systems.BossTrackingSystems;
using PathOfTerraria.Common.Systems.Questing;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Quests;

internal static class QuestUtils
{
	/// <summary> Safely spawns the given item either at the first NPC with the provided type, or at the player if none has been found. </summary>
	public static Item SpawnNPCQuestRewardItem(Player player, int npcType, int itemType, int stack = 1, bool noSync = false)
	{
		Entity entity = NPC.FindFirstNPC(npcType) is >= 0 and int npc ? Main.npc[npc] : player;
		int itemId = Item.NewItem(new EntitySource_Gift(entity), entity.Center, itemType, Stack: stack, noBroadcast: noSync);
		Item item = Main.item[itemId];

		if (!noSync && Main.netMode == NetmodeID.MultiplayerClient)
		{
			NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemId);
		}

		return item;
	}

	public static Func<QuestStep, bool> BossSkipCheck(int npcId)
	{
		return _ => BossTracker.TotalBossesDowned.Contains(npcId);
	}

	/// <summary>
	/// Returns true when the player should be offered a free re-grant of <paramref name="itemType"/>
	/// from a quest-giver: the quest is active, the player no longer holds the item, and the
	/// quest's <see cref="Quest.CurrentStep"/> is between the giveaway step and the step that
	/// counts as the boss being defeated. Once the player advances past <paramref name="lastActiveStep"/>
	/// (i.e. the boss has been killed) the regrant disappears so players have to farm maps instead.
	/// </summary>
	public static bool ShouldRegrantQuestItem<TQuest>(Player player, int itemType, int firstActiveStep, int lastActiveStep)
		where TQuest : Quest
	{
		string questName = ModContent.GetInstance<TQuest>().FullName;

		if (!player.GetModPlayer<QuestModPlayer>().QuestsByName.TryGetValue(questName, out Quest quest))
		{
			return false;
		}

		return quest.Active
			&& quest.CurrentStep >= firstActiveStep
			&& quest.CurrentStep <= lastActiveStep
			&& !player.HasItem(itemType);
	}

	/// <summary> Spawns and syncs a quest item gift from <paramref name="npc"/>. </summary>
	public static void GiftQuestItem(NPC npc, int itemType)
	{
		int item = Item.NewItem(new EntitySource_Gift(npc), npc.Hitbox, itemType, noGrabDelay: true);

		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item);
		}
	}
}
