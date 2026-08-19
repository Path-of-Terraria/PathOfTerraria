using SubworldLibrary;
using System.IO;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.QueenDomain;

internal class QueenSlimeSystem : ModSystem
{
	public override void NetSend(BinaryWriter writer)
	{
		if (SubworldSystem.Current is QueenSlimeDomain)
		{
			writer.Write(QueenSlimeDomain.CircleCenter.X);
			writer.Write(QueenSlimeDomain.CircleCenter.Y);
		}
	}

	public override void NetReceive(BinaryReader reader)
	{
		if (SubworldSystem.Current is QueenSlimeDomain)
		{
			QueenSlimeDomain.CircleCenter = new(reader.ReadInt16(), reader.ReadInt16());
		}
	}
}
