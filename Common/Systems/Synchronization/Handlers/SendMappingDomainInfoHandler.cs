using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.Maps;
using PathOfTerraria.Common.Systems.Sigils;
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
		// Never accept level, affix, or sigil state supplied by a client.
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

		int serializedSigilCount = reader.ReadByte();
		int sigilCount = Math.Min(serializedSigilCount, 4);
		var sigils = new SigilEntry[sigilCount];
		for (int i = 0; i < serializedSigilCount; i++)
		{
			SigilEntry entry = SigilEntry.NetReceive(reader);
			if (i < sigilCount)
			{
				sigils[i] = entry;
			}
		}
		SigilSystem.SetActive(sigils);

		MappingWorld.AreaLevel = level;
		MappingWorld.MapTier = tier;
	}
}
