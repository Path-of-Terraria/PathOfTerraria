using System.Collections.Generic;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Subworlds.BossDomains.SkeleDomain;

internal class RoomDatabase : ILoadable
{
	public Dictionary<int, RoomData> DataByRoomIndex = [];

	public void Load(Mod mod)
	{
		DataByRoomIndex.Clear();

		DataByRoomIndex.Add(0, new RoomData(WireColor.Yellow, OpeningType.Right, new Point(44, 29), new Point(23, 39), []));
		DataByRoomIndex.Add(1, new RoomData(WireColor.Blue, OpeningType.Left, new Point(89, 56), new Point(0, 11), 
			[new SpikeballInfo(new Point16(21, 46), 1.2f, false), new(new Point16(36, 46), 1.2f, false), new(new Point16(49, 46), 80, false), 
				new(new Point16(64, 46), 80, false)]));
	}

	public void Unload()
	{
	}
}
