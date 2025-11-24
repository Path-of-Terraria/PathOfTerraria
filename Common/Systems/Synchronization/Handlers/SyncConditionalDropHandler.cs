using PathOfTerraria.Common.NPCs.ConditionalDropping;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Handles syncing a conditional drop by sending it to the server and all other clients. This keeps the player's conditional drops consistent.
/// </summary>
internal class SyncConditionalDropHandler : Handler
{
	public static void Send(int id, bool add, int toClient = -1, int ignoreClient = -1)
	{
		ModPacket packet = Networking.GetPacket<SyncConditionalDropHandler>();
		packet.Write(id);
		packet.Write(add);
		packet.Send(toClient, ignoreClient);
	}

	internal override void Receive(BinaryReader reader, byte sender)
	{
		byte who = sender >= byte.MaxValue ? reader.ReadByte() : sender;
		SetValuesBasedOnReader(reader, who, Main.dedServ);
	}

	public static void SetValuesBasedOnReader(BinaryReader reader, byte who, bool runningOnServer)
	{
		int id = reader.ReadInt32();
		bool add = reader.ReadBoolean();
		Player plr = Main.player[who];

		if (add)
		{
			plr.GetModPlayer<ConditionalDropPlayer>().AddId(id, true);
		}
		else
		{
			plr.GetModPlayer<ConditionalDropPlayer>().RemoveId(id, true);
		}

		if (runningOnServer)
		{
			ModPacket packet = Networking.GetPacket(ModContent.GetInstance<SyncConditionalDropHandler>().Id);
			packet.Write(who);
			packet.Write(id);
			packet.Write(add);
			packet.Send();
		}
	}
}
