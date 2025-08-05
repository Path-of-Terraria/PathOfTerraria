using PathOfTerraria.Common.NPCs.ConditionalDropping;
using System.IO;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

/// <summary>
/// Handles syncing a conditional drop by sending it to the server and all other clients. This keeps the player's conditional drops consistent.
/// </summary>
internal class SyncConditionalDropHandler : Handler
{
	public override Networking.Message MessageType => Networking.Message.SyncConditionalDrop;

	/// <inheritdoc cref="Networking.Message.SyncConditionalDrop"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out byte who, out int id, out bool add);

		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Write(who);
		packet.Write(id);
		packet.Write(add);
		packet.Send();
	}

	internal override void ServerRecieve(BinaryReader reader)
	{
		SetValuesBasedOnReader(reader, true);
	}

	internal override void ClientRecieve(BinaryReader reader)
	{
		SetValuesBasedOnReader(reader, false);
	}

	private void SetValuesBasedOnReader(BinaryReader reader, bool sync)
	{
		byte who = reader.ReadByte();
		int id = reader.ReadInt32();
		bool add = reader.ReadBoolean();
		Player plr = Main.player[who];

		if (add)
		{
			plr.GetModPlayer<ConditionalDropPlayer>().AddId(id);
		}
		else
		{
			plr.GetModPlayer<ConditionalDropPlayer>().RemoveId(id);
		}

		if (sync)
		{
			ModPacket packet = Networking.GetPacket(MessageType);
			packet.Write(who);
			packet.Write(id);
			packet.Write(add);
			packet.Send();
		}
	}
}
