using PathOfTerraria.Common.Events;
using PathOfTerraria.Common.Subworlds;
using SubworldLibrary;
using System.Collections.Generic;
using System.IO;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <inheritdoc cref="Networking.Message.RequestMappingTiers"/>
internal class RequestMappingTierHandler : Handler
{
	public override Networking.Message MessageType => Networking.Message.RequestMappingTiers;

	public override void Load(Mod mod)
	{
		base.Load(mod);

		PathOfTerrariaPlayerEvents.OnEnterWorldEvent += player =>
		{
			if (Main.netMode != NetmodeID.SinglePlayer && SubworldSystem.Current is MappingWorld)
			{
				ModContent.GetInstance<RequestMappingTierHandler>().Send((byte)player.whoAmI);
			}
		};
	}

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
