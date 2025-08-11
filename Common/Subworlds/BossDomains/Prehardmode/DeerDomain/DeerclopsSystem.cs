using System.IO;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode.DeerDomain;

internal class DeerclopsSystem : ModSystem
{
	public Point16 AntlerLocation = Point16.Zero;

	public override void SaveWorldData(TagCompound tag)
	{
		tag.Add("antler", AntlerLocation);
	}

	public override void LoadWorldData(TagCompound tag)
	{
		AntlerLocation = tag.Get<Point16>("antler");
	}

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write(AntlerLocation.X);
		writer.Write(AntlerLocation.Y);
	}

	public override void NetReceive(BinaryReader reader)
	{
		AntlerLocation = new Point16(reader.ReadInt16(), reader.ReadInt16());
	}
}
