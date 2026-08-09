using PathOfTerraria.Common.Systems.Synchronization.Handlers;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Questing;

internal sealed class QuestKillGlobalNPC : GlobalNPC
{
	public override void OnKill(NPC npc)
	{
		NPC.HitInfo hit = default;

		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			return;
		}

		for (int i = 0; i < npc.playerInteraction.Length; i++)
		{
			if (!npc.playerInteraction[i])
			{
				continue;
			}

			Player player = Main.player[i];

			if (!player.active)
			{
				continue;
			}

			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				player.GetModPlayer<QuestModPlayer>().OnKillNPC(npc, hit, 0);
			}
			else
			{
				SyncQuestKillCredit.Send((byte)i, npc.type, npc.netID);
			}
		}
	}
}
