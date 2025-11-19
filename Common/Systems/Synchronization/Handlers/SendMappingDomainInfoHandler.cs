using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using System.Collections.Generic;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Sends mapping domain info (Level, Tier, Affixes) to the server.
/// </summary>
internal class SendMappingDomainInfoHandler : Handler
{
	public static void Send(short level, short tier, List<MapAffix> affixes)
	{
		ModPacket packet = Networking.GetPacket<SendMappingDomainInfoHandler>();
		packet.Write(level);
		packet.Write(tier);
		packet.Write((byte)affixes.Count);

		foreach (MapAffix item in affixes)
		{
			item.NetSend(packet);
		}

		packet.Send();
	}

	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		GetAndSetMappingDomainInfo(reader);
	}

	internal static void GetAndSetMappingDomainInfo(BinaryReader reader)
	{
		short level = reader.ReadInt16();
		short tier = reader.ReadInt16();
		byte count = reader.ReadByte();

		MappingWorld.Affixes = [];

		for (int i = 0; i < count; ++i)
		{
			Affix affix = Affix.FromBReader(reader);
			MappingWorld.Affixes.Add((MapAffix)affix);
		}

		MappingWorld.AreaLevel = level;
		MappingWorld.MapTier = tier;
	}
}
