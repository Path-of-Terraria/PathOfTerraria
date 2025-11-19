using PathOfTerraria.Common.Subworlds;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Adds a tier to the completion tracker. Meant to be used through <see cref="Networking.SendPacketToMainServer(ModPacket, string)"/>.<br/>
/// <b>Note:</b> This packet sends an additional short, count, to clients so that their downed count is forcefully set to the server's instead of allowing a desync.<br/>
/// This should not affect any normal use of this packet however.
/// </summary>
internal class SendMappingTierHandler : Handler
{
	public static void Send(short tier)
	{
		ModPacket packet = Networking.GetPacket<SendMappingTierHandler>(3);
		packet.Write(tier);
		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		short tier = reader.ReadInt16();
		MappingDomainSystem.TiersDownedTracker tracker = ModContent.GetInstance<MappingDomainSystem>().Tracker;
		tracker.AddCompletion(tier);

		ModPacket packet = Networking.GetPacket(Id, 5);
		packet.Write(tier);
		packet.Write((short)tracker.GetCompletions()[tier]);
		packet.Send();
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		short tier = reader.ReadInt16();
		short count = reader.ReadInt16();
		MappingDomainSystem.TiersDownedTracker tracker = ModContent.GetInstance<MappingDomainSystem>().Tracker;

		tracker.SetCompletion(tier, count);
	}
}
