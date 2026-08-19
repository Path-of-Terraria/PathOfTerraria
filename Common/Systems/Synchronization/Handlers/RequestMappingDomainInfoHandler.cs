using System.IO;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.Affixes.Maps;
using SubworldLibrary;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.Affixes.Maps;
using PathOfTerraria.Common.Systems.Sigils;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Requests mapping domain info (Level, Tier, Affixes) from the server. Sent only from clients.
/// </summary>
internal class RequestMappingDomainInfoHandler : Handler
{
	public sealed class RequestMappingDomainInfoHandlerPlayer : ModPlayer
	{
		public override void OnEnterWorld()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient && SubworldSystem.Current is MappingWorld)
			{
				Send();
			}
		}
	}

	public static void Send()
	{
		Networking.GetPacket<RequestMappingDomainInfoHandler>().Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		if (sender >= Main.maxPlayers || !Main.player[sender].active || SubworldSystem.Current is not MappingWorld)
		{
			return;
		}

		ModPacket packet = Networking.GetPacket(Id);
		packet.Write((short)MappingWorld.AreaLevel);
		packet.Write((short)MappingWorld.MapTier);
		IReadOnlyList<MapAffix> affixes = MappingWorld.Affixes ?? [];
		packet.Write((byte)Math.Min(affixes.Count, byte.MaxValue));

		foreach (MapAffix item in affixes.Take(byte.MaxValue))
		{
			item.NetSend(packet);
		}

		SigilSystem.WriteActive(packet);

		packet.Send(sender);
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		SendMappingDomainInfoHandler.GetAndSetMappingDomainInfo(reader);
	}
}
