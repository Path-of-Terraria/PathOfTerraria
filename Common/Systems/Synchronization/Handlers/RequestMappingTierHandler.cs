using System.Collections.Generic;
using System.IO;
using PathOfTerraria.Common.Subworlds;
using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Requests all mapping tier downs from the server.
/// </summary>
internal class RequestMappingTierHandler : Handler
{
	public sealed class RequestMappingTierHandlerPlayer : ModPlayer
	{
		public override void OnEnterWorld()
		{
			if (Main.netMode != NetmodeID.SinglePlayer && SubworldSystem.Current is MappingWorld)
			{
				Send();
			}
		}
	}

	public static void Send()
	{
		Networking.GetPacket<RequestMappingTierHandler>().Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		MappingDomainSystem.TiersDownedTracker tracker = ModContent.GetInstance<MappingDomainSystem>().Tracker;

		ModPacket packet = Networking.GetPacket(Id);
		Dictionary<int, int> tierCompletions = tracker.GetCompletions();
		packet.Write((short)tierCompletions.Count);

		foreach ((int tier, int comp) in tierCompletions)
		{
			packet.Write((short)tier);
			packet.Write((short)comp);
		}

		packet.Send(sender);
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		short count = reader.ReadInt16();
		MappingDomainSystem.TiersDownedTracker tracker = ModContent.GetInstance<MappingDomainSystem>().Tracker;

		for (int i = 0; i < count; ++i)
		{
			tracker.SetCompletion(reader.ReadInt16(), reader.ReadInt16());
		}
	}
}
