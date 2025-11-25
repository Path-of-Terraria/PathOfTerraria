using System.IO;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using SubworldLibrary;
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
			if (Main.netMode != NetmodeID.SinglePlayer && SubworldSystem.Current is MappingWorld)
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
		ModPacket packet = Networking.GetPacket(Id);
		packet.Write((short)MappingWorld.AreaLevel);
		packet.Write((short)MappingWorld.MapTier);
		packet.Write((byte)MappingWorld.Affixes.Count);

		foreach (MapAffix item in MappingWorld.Affixes)
		{
			item.NetSend(packet);
		}

		packet.Send(sender);
	}

	internal override void ClientReceive(BinaryReader reader, byte sender)
	{
		SendMappingDomainInfoHandler.GetAndSetMappingDomainInfo(reader);
	}
}
