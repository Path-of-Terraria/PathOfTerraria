using SubworldLibrary;
using System.IO;

namespace PathOfTerraria.Common.Subworlds.BossDomains.SlimeDomain;

internal class KingDomainSystem : ModSystem
{
	public static bool InDomain => SubworldSystem.Current is KingSlimeDomain;
	public static KingSlimeDomain Domain => SubworldSystem.Current as KingSlimeDomain;

	public override void NetSend(BinaryWriter writer)
	{
		if (!InDomain)
		{
			return;
		}

		writer.Write((short)Domain.Arena.X);
		writer.Write((short)Domain.Arena.Y);
		writer.Write((short)Domain.Arena.Width);
		writer.Write((short)Domain.Arena.Height);
	}

	public override void NetReceive(BinaryReader reader)
	{
		if (!InDomain)
		{
			return;
		}

		int x = reader.ReadInt16();
		int y = reader.ReadInt16();
		int width = reader.ReadInt16();
		int height = reader.ReadInt16();

		Domain.Arena = new Rectangle(x, y, width, height);
	}
}
