using PathOfTerraria.Common.Systems.Questing;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Sends quest kill credit for an NPC death to the owning client of the quest progress.
/// </summary>
internal class SyncQuestKillCredit : Handler
{
	public static void Send(byte targetPlayer, int npcType, int npcNetId)
	{
		ModPacket packet = Networking.GetPacket<SyncQuestKillCredit>();
		packet.Write((short)npcType);
		packet.Write((short)npcNetId);
		packet.Send(targetPlayer);
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		int npcType = reader.ReadInt16();
		int npcNetId = reader.ReadInt16();
		NPC.HitInfo hit = default;

		Main.LocalPlayer.GetModPlayer<QuestModPlayer>().OnKillNPC(npcType, npcNetId, hit, 0);
	}
}
