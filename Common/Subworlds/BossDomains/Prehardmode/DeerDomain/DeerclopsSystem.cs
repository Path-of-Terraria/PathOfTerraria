using System.IO;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode.DeerDomain;

internal class DeerclopsSystem : ModSystem
{
	public Point16 AntlerLocation = Point16.Zero;

	public override void SaveWorldData(TagCompound tag)
	{
		tag["antler"] = AntlerLocation;
	}

	public override void LoadWorldData(TagCompound tag)
	{
		AntlerLocation = tag.Get<Point16>("antler");
	}

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write((ushort)AntlerLocation.X);
		writer.Write((ushort)AntlerLocation.Y);
	}

	public override void NetReceive(BinaryReader reader)
	{
		ushort x = reader.ReadUInt16();
		ushort y = reader.ReadUInt16();

		AntlerLocation = new(x, y);
	}
}
