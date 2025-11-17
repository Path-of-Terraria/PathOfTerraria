using PathOfTerraria.Common.Systems.Questing;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

internal class SyncPlayerQuestActive : Handler
{
	public override Networking.Message MessageType => Networking.Message.PlayerQuestActive;

	public static void Send(string quest, bool enabled, int toClient = -1, int ignoreClient = -1)
	{
		ModPacket packet = Networking.GetPacket(Networking.Message.PlayerQuestActive);
		packet.Write(quest);
		packet.Write(enabled);
		packet.Send(toClient, ignoreClient);
	}

	internal override void Receive(BinaryReader reader, byte sender)
	{
		string questName = reader.ReadString();
		bool enabled = reader.ReadBoolean();

		if (enabled)
		{
			Main.player[sender].GetModPlayer<QuestModPlayer>().EnabledQuestsByName.Add(questName);
		}
		else
		{
			Main.player[sender].GetModPlayer<QuestModPlayer>().EnabledQuestsByName.Remove(questName);
		}

		if (Main.dedServ)
		{
			Send(questName, enabled, -1, sender);
		}
	}
}
