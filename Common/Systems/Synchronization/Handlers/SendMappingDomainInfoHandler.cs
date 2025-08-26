using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using System.Collections.Generic;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <inheritdoc cref="Networking.Message.SendMappingDomainInfo"/>
internal class SendMappingDomainInfoHandler : Handler
{
	public override Networking.Message MessageType => Networking.Message.SendMappingDomainInfo;

	/// <inheritdoc cref="Networking.Message.SendMappingDomainInfo"/>
	public override void Send(params object[] parameters)
	{
		CastParameters(parameters, out short level, out short tier, out List<MapAffix> affixes);

		ModPacket packet = Networking.GetPacket(MessageType);
		packet.Write(level);
		packet.Write(tier);
		packet.Write((byte)affixes.Count);

		foreach (MapAffix item in affixes)
		{
			item.NetSend(packet);
		}

		packet.Send();
	}

	internal override void ServerRecieve(BinaryReader reader)
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
