using PathOfTerraria.Common.Subworlds;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <inheritdoc cref="Networking.Message.SendMappingTierDown"/>
internal class SendMappingTierHandler : Handler
{
	public override Networking.Message MessageType => Networking.Message.SendMappingTierDown;

	/// <inheritdoc cref="Networking.Message.SendMappingTierDown"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out short tier);

		ModPacket packet = Networking.GetPacket(MessageType, 3);
		packet.Write(tier);
		packet.Send();
	}

	internal override void ServerRecieve(BinaryReader reader)
	{
		short tier = reader.ReadInt16();
		MappingDomainSystem.TiersDownedTracker tracker = ModContent.GetInstance<MappingDomainSystem>().Tracker;
		tracker.AddCompletion(tier);

		ModPacket packet = Networking.GetPacket(MessageType, 5);
		packet.Write(tier);
		packet.Write((short)tracker.GetCompletions()[tier]);
		packet.Send();
	}

	internal override void ClientRecieve(BinaryReader reader)
	{
		short tier = reader.ReadInt16();
		short count = reader.ReadInt16();
		MappingDomainSystem.TiersDownedTracker tracker = ModContent.GetInstance<MappingDomainSystem>().Tracker;

		tracker.SetCompletion(tier, count);
	}
}
