using PathOfTerraria.Common.Systems.ExperienceSystem;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Spawns experience on all clients.
/// </summary>
internal class ExperienceHandler : Handler
{
	public static void Send(byte target, int xpValue, Vector2 position, Vector2 velocity)
	{
		ModPacket packet = Networking.GetPacket<ExperienceHandler>();
		packet.Write(target);
		packet.Write(xpValue);
		packet.WriteVector2(position);
		packet.WriteVector2(velocity);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		ModPacket packet = Networking.GetPacket(Id);
		byte target = reader.ReadByte();

		packet.Write(target);
		packet.Write(reader.ReadInt32());
		packet.WriteVector2(reader.ReadVector2());
		packet.WriteVector2(reader.ReadVector2());
		packet.Send(-1, target);
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		int target = reader.ReadByte();
		int xp = reader.ReadInt32();
		Vector2 position = reader.ReadVector2();
		Vector2 velocity = reader.ReadVector2();

		ExperienceTracker.SpawnExperience(xp, position, velocity, target, true);
	}
}
