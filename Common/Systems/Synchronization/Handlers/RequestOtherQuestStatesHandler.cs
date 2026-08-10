using PathOfTerraria.Common.Systems.Questing;
using System.Collections.Generic;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Requests the cached quest state of every other active player from the server.
/// </summary>
internal class RequestOtherQuestStatesHandler : Handler
{
	public static void Send()
	{
		Networking.GetPacket<RequestOtherQuestStatesHandler>().Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		if (sender >= Main.maxPlayers || !Main.player[sender].active)
		{
			return;
		}

		foreach (Player player in Main.ActivePlayers)
		{
			if (player.whoAmI == sender)
			{
				continue;
			}

			QuestModPlayer questPlayer = player.GetModPlayer<QuestModPlayer>();
			ModPacket packet = Networking.GetPacket(Id);
			packet.Write((byte)player.whoAmI);
			packet.Write((ushort)questPlayer.ActiveQuestStepsByName.Count);

			foreach (KeyValuePair<string, string> quest in questPlayer.ActiveQuestStepsByName)
			{
				packet.Write(quest.Key);
				packet.Write(quest.Value);
			}

			packet.Write((ushort)questPlayer.CompletedQuestsByName.Count);

			foreach (string questName in questPlayer.CompletedQuestsByName)
			{
				packet.Write(questName);
			}

			packet.Send(sender);
		}
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		byte playerWhoAmI = reader.ReadByte();

		if (playerWhoAmI >= Main.maxPlayers)
		{
			return;
		}

		QuestModPlayer questPlayer = Main.player[playerWhoAmI].GetModPlayer<QuestModPlayer>();
		questPlayer.EnabledQuestsByName.Clear();
		questPlayer.ActiveQuestStepsByName.Clear();
		questPlayer.CompletedQuestsByName.Clear();

		ushort activeCount = reader.ReadUInt16();

		for (int i = 0; i < activeCount; i++)
		{
			string questName = reader.ReadString();
			string activeStep = reader.ReadString();
			questPlayer.SetSyncedQuestState(questName, true, activeStep, false);
		}

		ushort completedCount = reader.ReadUInt16();

		for (int i = 0; i < completedCount; i++)
		{
			questPlayer.SetSyncedQuestState(reader.ReadString(), false, string.Empty, true);
		}
	}
}
