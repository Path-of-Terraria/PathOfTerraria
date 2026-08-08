using PathOfTerraria.Common.Systems.Questing;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Syncs a player's active/completed quest state and current active step.
/// </summary>
internal class SyncPlayerQuestActive : Handler
{
	public static void Send(string quest, bool active, string activeStep, bool completed, int toClient = -1, int ignoreClient = -1)
	{
		ModPacket packet = Networking.GetPacket<SyncPlayerQuestActive>();
		WriteState(packet, quest, active, activeStep, completed);
		packet.Send(toClient, ignoreClient);
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		ReadState(reader, out string questName, out bool active, out string activeStep, out bool completed);
		Main.player[sender].GetModPlayer<QuestModPlayer>().SetSyncedQuestState(questName, active, activeStep, completed);

		ModPacket packet = Networking.GetPacket(Id);
		packet.Write(sender);
		WriteState(packet, questName, active, activeStep, completed);
		packet.Send(-1, sender);
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		byte playerWhoAmI = reader.ReadByte();
		ReadState(reader, out string questName, out bool active, out string activeStep, out bool completed);
		Main.player[playerWhoAmI].GetModPlayer<QuestModPlayer>().SetSyncedQuestState(questName, active, activeStep, completed);
	}

	private static void WriteState(BinaryWriter writer, string questName, bool active, string activeStep, bool completed)
	{
		writer.Write(questName);
		writer.Write(active);
		writer.Write(activeStep);
		writer.Write(completed);
	}

	private static void ReadState(BinaryReader reader, out string questName, out bool active, out string activeStep, out bool completed)
	{
		questName = reader.ReadString();
		active = reader.ReadBoolean();
		activeStep = reader.ReadString();
		completed = reader.ReadBoolean();
	}
}
