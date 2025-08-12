using PathOfTerraria.Common.NPCs.ConditionalDropping;
using System.IO;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

/// <summary>
/// Handles syncing a conditional drop by sending it to the server and all other clients. This keeps the player's conditional drops consistent.
/// </summary>
internal class SyncNewConditionalDropPlayerHandler : Handler
{
	public override Networking.Message MessageType => Networking.Message.SyncNewConditionalDropPlayer;

	/// <inheritdoc cref="Networking.Message.SyncNewConditionalDropPlayer"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out byte who, out int[] types);

		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Write(who);
		packet.Write((byte)types.Length);

		for (int i = 0; i < types.Length; i++)
		{
			packet.Write(types[i]);
		}

		packet.Send();
	}

	internal override void ServerRecieve(BinaryReader reader)
	{
		int who = reader.ReadByte();
		byte length = reader.ReadByte();
		Player plr = Main.player[who];

		for (int i = 0; i < length; ++i)
		{
			int id = reader.ReadInt32();
			plr.GetModPlayer<ConditionalDropPlayer>().AddId(id, true);

			ModPacket packet = Networking.GetPacket(Networking.Message.SyncConditionalDrop);
			packet.Write(who);
			packet.Write(id);
			packet.Write(true);
			packet.Send(-1, who);
		}
	}
}
