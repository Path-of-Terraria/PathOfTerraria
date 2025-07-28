using PathOfTerraria.Common.Systems.ExperienceSystem;
using System.IO;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

internal class ExperienceHandler : Handler
{
	public override Networking.Message MessageType => Networking.Message.SpawnExperience;

	/// <summary>
	/// Spawns an orb on all clients but the target (parameter 0).
	/// Generally, this should only be called by <see cref="ExperienceTracker.SpawnExperience(int, Vector2, Vector2, int, bool)"/>.<br/>
	/// Signature:<br/><c>byte target, int xpValue, Vector2 position, Vector2 velocity, bool spawnLocally = false</c>
	/// </summary>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out byte target, out int xpValue, out Vector2 position, out Vector2 velocity);
		
		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Write(target);
		packet.Write(xpValue);
		packet.WriteVector2(position);
		packet.WriteVector2(velocity);
		packet.Send();

		if (GetOptionalBool(parameters, 4))
		{
			ExperienceTracker.SpawnExperience(xpValue, position, velocity, target, true);
		}
	}

	internal override void ServerRecieve(BinaryReader reader)
	{
		ModPacket packet = Networking.GetPacket(MessageType);
		byte target = reader.ReadByte();

		packet.Write(target);
		packet.Write(reader.ReadInt32());
		packet.WriteVector2(reader.ReadVector2());
		packet.WriteVector2(reader.ReadVector2());
		packet.Send(-1, target);
	}

	internal override void ClientRecieve(BinaryReader reader)
	{
		int target = reader.ReadByte();
		int xp = reader.ReadInt32();
		Vector2 position = reader.ReadVector2();
		Vector2 velocity = reader.ReadVector2();

		ExperienceTracker.SpawnExperience(xp, position, velocity, target, true);
	}
}
