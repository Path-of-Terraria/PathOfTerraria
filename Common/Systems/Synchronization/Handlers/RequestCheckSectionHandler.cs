using System.IO;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Calls <see cref="RemoteClient.CheckSection(int, Vector2, int)"/> on the server for the given player.
/// </summary>
internal class RequestCheckSectionHandler : Handler
{
	public static void Send(Vector2 position, int fluff = 1)
	{
		ModPacket packet = Networking.GetPacket<RequestCheckSectionHandler>();
		packet.Write(position.X);
		packet.Write(position.Y);
		packet.Write((byte)fluff);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		Vector2 pos = reader.ReadVector2();
		byte fluff = reader.ReadByte();

		RemoteClient.CheckSection(sender, pos, fluff);
		NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, sender, pos.X, pos.Y, 2, 1);
	}
}
