using PathOfTerraria.Common.NPCs.ConditionalDropping;
using System.IO;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

/// <summary>
/// Handles syncing a conditional drop by sending it to the server. Doesn't do anything if recieved by a client, as drops are server-side only in multiplayer.
/// </summary>
internal class SyncConditionalDropHandler : Handler
{
	public override Networking.Message MessageType => Networking.Message.SyncConditionalDrop;

	/// <inheritdoc cref="Networking.Message.SyncConditionalDrop"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out int id, out bool add);

		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Write(id);
		packet.Write(add);
		packet.Send();
	}

	internal override void ServerRecieve(BinaryReader reader)
	{
		int id = reader.ReadInt32();
		bool add = reader.ReadBoolean();

		if (add)
		{
			ConditionalDropHandler.AddId(id);
		}
		else
		{
			ConditionalDropHandler.RemoveId(id);
		}
	}
}
