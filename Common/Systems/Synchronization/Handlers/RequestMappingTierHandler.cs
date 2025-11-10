using System.Collections.Generic;
using System.IO;
using PathOfTerraria.Common.Subworlds;
using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <inheritdoc cref="Networking.Message.RequestMappingTiers"/>
internal class RequestMappingTierHandler : Handler
{
	public sealed class RequestMappingTierHandlerPlayer : ModPlayer
	{
		public override void OnEnterWorld()
		{
			if (Main.netMode != NetmodeID.SinglePlayer && SubworldSystem.Current is MappingWorld)
			{
				ModContent.GetInstance<RequestMappingTierHandler>().Send((byte)Player.whoAmI);
			}
		}
	}

	public override Networking.Message MessageType => Networking.Message.RequestMappingTiers;

	/// <inheritdoc cref="Networking.Message.RequestMappingTiers"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out byte who);

		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Write(who);
		packet.Send();
	}

	internal override void ServerRecieve(BinaryReader reader)
	{
		byte who = reader.ReadByte();
		MappingDomainSystem.TiersDownedTracker tracker = ModContent.GetInstance<MappingDomainSystem>().Tracker;

		ModPacket packet = Networking.GetPacket(MessageType);
		Dictionary<int, int> tierCompletions = tracker.GetCompletions();
		packet.Write((short)tierCompletions.Count);

		foreach ((int tier, int comp) in tierCompletions)
		{
			packet.Write((short)tier);
			packet.Write((short)comp);
		}

		packet.Send(who);
	}

	internal override void ClientRecieve(BinaryReader reader)
	{
		short count = reader.ReadInt16();
		MappingDomainSystem.TiersDownedTracker tracker = ModContent.GetInstance<MappingDomainSystem>().Tracker;

		for (int i = 0; i < count; ++i)
		{
			tracker.SetCompletion(reader.ReadInt16(), reader.ReadInt16());
		}
	}
}
