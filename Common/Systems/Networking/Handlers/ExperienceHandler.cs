using PathOfTerraria.Common.Systems.ExperienceSystem;
using System.IO;

namespace PathOfTerraria.Common.Systems.Networking.Handlers;

internal class ExperienceHandler : Handler
{
	public override Networking.Message MessageType => Networking.Message.SpawnExperience;

	/// <inheritdoc cref="Networking.Message.SpawnExperience"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out byte target, out int xpValue, out Vector2 position, out Vector2 velocity);
		
		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Write(target);
		packet.Write(xpValue);
		packet.WriteVector2(position);
		packet.WriteVector2(velocity);
		packet.Send();

		if (TryGetOptionalValue(parameters, 4, out bool runLocally) && runLocally)
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
