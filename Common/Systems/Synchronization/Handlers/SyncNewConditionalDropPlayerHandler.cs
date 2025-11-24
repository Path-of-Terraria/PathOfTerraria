using PathOfTerraria.Common.NPCs.ConditionalDropping;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Handles syncing a conditional drop by sending it to the server and all other clients. This keeps the player's conditional drops consistent.
/// </summary>
internal class SyncNewConditionalDropPlayerHandler : Handler
{
	public static void Send(int[] types)
	{
		ModPacket packet = Networking.GetPacket<SyncNewConditionalDropPlayerHandler>();
		packet.Write((byte)types.Length);

		for (int i = 0; i < types.Length; i++)
		{
			packet.Write(types[i]);
		}

		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		byte length = reader.ReadByte();
		Player plr = Main.player[sender];

		for (int i = 0; i < length; ++i)
		{
			int id = reader.ReadInt32();
			plr.GetModPlayer<ConditionalDropPlayer>().AddId(id, true);

			ModPacket packet = Networking.GetPacket(Id);
			packet.Write(sender);
			packet.Write(id);
			packet.Write(true);
			packet.Send(-1, sender);
		}
	}
}
