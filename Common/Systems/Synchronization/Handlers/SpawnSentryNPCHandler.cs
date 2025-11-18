using PathOfTerraria.Common.NPCs;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <inheritdoc cref="Networking.Message.SpawnSentryNPC"/>
internal class SpawnSentryNPCHandler : Handler
{
	/// <inheritdoc cref="Networking.Message.SpawnSentryNPC"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out ushort type, out byte owner, out Vector2 position, out ushort timeLeft, out ushort damage);

		ModPacket packet = Networking.GetPacket(Id);
		packet.Write(type);
		packet.Write(owner);
		packet.WriteVector2(position);
		packet.Write(timeLeft);
		packet.Write(damage);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		ushort type = reader.ReadUInt16();
		byte who = reader.ReadByte();
		Vector2 position = reader.ReadVector2();
		ushort timeLeft = reader.ReadUInt16();
		ushort damage = reader.ReadUInt16();

		NPC npc = SentryNPC.Spawn(type, Main.player[who], position, timeLeft);
		npc.damage = damage;
	}
}
