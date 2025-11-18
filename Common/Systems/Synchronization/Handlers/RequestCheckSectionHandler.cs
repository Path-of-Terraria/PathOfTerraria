using System.IO;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <inheritdoc cref="Networking.Message.RequestCheckSection"/>
internal class RequestCheckSectionHandler : Handler
{
	/// <inheritdoc cref="Networking.Message.RequestCheckSection"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out byte player, out Vector2 position);

		ModPacket packet = Networking.GetPacket(Id);
		packet.Write(player);
		packet.Write(position.X);
		packet.Write(position.Y);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		byte plr = reader.ReadByte();
		Vector2 pos = reader.ReadVector2();

		RemoteClient.CheckSection(plr, pos);
		NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, plr, pos.X, pos.Y, 2, 1);
	}
}
