using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.BossDomains.SkeleDomain;

internal class RoomDatabase : ModSystem
{
	public Dictionary<int, RoomData> DataByRoomIndex = [];

	private readonly List<EngageTimerInfo> _timers = [];

	public void PlaceRoom(int id, int x, int y, Vector2 origin)
	{
		DataByRoomIndex[id].PlaceRoom(x, y, id, origin);
	}

	public void AddTimerInfo(List<EngageTimerInfo> info)
	{
		_timers.AddRange(info);
	}

	public override void Load()
	{
		DataByRoomIndex.Clear();
		DataByRoomIndex.Add(0, new RoomData(WireColor.Yellow, OpeningType.Right, new Point(44, 29), new Point(23, 39), null, [new EngageTimerInfo(new Point16(41, 6), 0)]));

		DataByRoomIndex.Add(1, new RoomData(WireColor.Blue, OpeningType.Left, new Point(89, 56), new Point(0, 11), 
			[new SpikeballInfo(new Point16(21, 46), 80, false), new(new Point16(36, 46), 80, false), new(new Point16(49, 46), 80, false), 
				new SpikeballInfo(new Point16(64, 46), 80, false)],
			[new EngageTimerInfo(new Point16(87, 6), 0), new EngageTimerInfo(new Point16(90, 12), 60)]));

		DataByRoomIndex.Add(2, new RoomData(WireColor.Green, OpeningType.Above, new Point(33, 0), new Point(1, -1),
			[new SpikeballInfo(new Point16(15, 59), 110, false), new(new Point16(49, 59), 110, false)],
			[new EngageTimerInfo(new Point16(18, 27), 0), new EngageTimerInfo(new Point16(20, 27), 45), new EngageTimerInfo(new Point16(22, 27), 90),
				new EngageTimerInfo(new Point16(24, 27), 135), 
				new EngageTimerInfo(new Point16(39, 34), 0), new EngageTimerInfo(new Point16(40, 34), 60), new EngageTimerInfo(new Point16(41, 34), 120)]));
	}

	public override void PreUpdateWorld()
	{
		for (int i = 0; i < _timers.Count; ++i)
		{
			EngageTimerInfo info = _timers[i];

			info.Ticks--;

			if (info.Ticks <= 0)
			{
				Tile tile = Main.tile[info.Position];
				tile.TileFrameY = 18;

				Wiring.CheckMech(info.Position.X, info.Position.Y, 18000);
			}
		}

		_timers.RemoveAll(x => x.Ticks <= 0);
	}
}
