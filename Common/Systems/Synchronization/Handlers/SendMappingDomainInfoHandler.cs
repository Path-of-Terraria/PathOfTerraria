using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.Maps;
using PathOfTerraria.Common.Systems.Scarabs;
using System.IO;

namespace PathOfTerraria.Common.Systems.Synchronization.Handlers;

/// <summary>
/// Applies server-authored mapping domain info received by a client.
/// </summary>
internal class SendMappingDomainInfoHandler : Handler
{
	internal override void ServerReceive(BinaryReader reader, byte sender)
	{
		// Map state is derived from the server's MapDeviceEntity when portal entry is authorized.
		// Never accept level, affix, or scarab state supplied by a client.
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

		int serializedScarabCount = reader.ReadByte();
		int scarabCount = Math.Min(serializedScarabCount, 4);
		var scarabs = new ScarabEntry[scarabCount];
		for (int i = 0; i < serializedScarabCount; i++)
		{
			ScarabEntry entry = ScarabEntry.NetReceive(reader);
			if (i < scarabCount)
			{
				scarabs[i] = entry;
			}
		}
		ScarabSystem.SetActive(scarabs);

		MappingWorld.AreaLevel = level;
		MappingWorld.MapTier = tier;
	}
}
