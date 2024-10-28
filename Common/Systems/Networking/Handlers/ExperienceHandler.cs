using PathOfTerraria.Common.Systems.Experience;
using System.IO;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

internal static class ExperienceHandler
{
	/// <summary>
	/// Spawns an orb on all clients but <paramref name="target"/>. 
	/// Generally, this should only be called by <see cref="ExperienceModSystem.SpawnExperience(int, Vector2, Vector2, int, bool)"/>.
	/// </summary>
	/// <param name="target">Target for the exp to aim for.</param>
	/// <param name="xpValue">Amount of XP to spawn.</param>
	/// <param name="position">Where it spawns.</param>
	/// <param name="velocity">The base velocity of the spawned exp.</param>
	public static void SendExperience(byte target, int xpValue, Vector2 position, Vector2 velocity, bool spawnLocally = false)
	{
		ModPacket packet = Networking.GetPacket(Networking.Message.SpawnExperience);

		packet.Write(target);
		packet.Write(xpValue);
		packet.WriteVector2(position);
		packet.WriteVector2(velocity);
		packet.Send();

		if (spawnLocally)
		{
			ExperienceTracker.SpawnExperience(xpValue, position, velocity, target, true);
		}
	}

	internal static void ServerRecieve(BinaryReader reader)
	{
		ModPacket packet = Networking.GetPacket(Networking.Message.SpawnExperience);
		int target = reader.ReadByte();

		packet.Write(target);
		packet.Write(reader.ReadInt32());
		packet.WriteVector2(reader.ReadVector2());
		packet.WriteVector2(reader.ReadVector2());
		packet.Send(-1, target);
	}

	internal static void ClientRecieve(BinaryReader reader)
	{
		int target = reader.ReadByte();
		int xp = reader.ReadInt32();
		Vector2 position = reader.ReadVector2();
		Vector2 velocity = reader.ReadVector2();

		ExperienceTracker.SpawnExperience(xp, position, velocity, target, true);
	}
}
