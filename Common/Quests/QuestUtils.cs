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
}
