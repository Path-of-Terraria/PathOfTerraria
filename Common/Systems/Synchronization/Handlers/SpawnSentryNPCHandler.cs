using PathOfTerraria.Common.NPCs;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Spawns a Sentry NPC on the server. All NPCs need to be spawned on the server.
/// </summary>
internal class SpawnSentryNPCHandler : Handler
{
	public static void Send(ushort type, Vector2 position, ushort timeLeft, ushort damage, int toClient = -1, int ignoreClient = -1)
	{
		ModPacket packet = Networking.GetPacket<SpawnSentryNPCHandler>();
		packet.Write(type);
		packet.WriteVector2(position);
		packet.Write(timeLeft);
		packet.Write(damage);
		packet.Send(toClient, ignoreClient);
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		ushort type = reader.ReadUInt16();
		Vector2 position = reader.ReadVector2();
		ushort timeLeft = reader.ReadUInt16();
		ushort damage = reader.ReadUInt16();

		NPC npc = SentryNPC.Spawn(type, Main.player[sender], position, timeLeft);
		npc.damage = damage;
	}
}
