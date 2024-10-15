using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Subworlds.BossDomains.SkeleDomain;

public record PlacedRoom(RoomData Data, Rectangle Area);

internal class RoomDatabase : ModSystem
{
	public Dictionary<int, RoomData> DataByRoomIndex = [];

	private readonly List<EngageTimerInfo> _timers = [];

	public static PlacedRoom PlaceRandomRoom(OpeningType opening, int x, int y, List<WireColor> usedColors)
	{
		RoomDatabase instance = ModContent.GetInstance<RoomDatabase>();
		IEnumerable<KeyValuePair<int, RoomData>> roomDatas = instance.DataByRoomIndex.Where(x => x.Value.Opening == opening);

		int roomId = WorldGen.genRand.Next(roomDatas.Count());
		KeyValuePair<int, RoomData> roomData = roomDatas.ElementAt(roomId);

		while (usedColors.Contains(roomData.Value.Wire))
		{
			roomId = WorldGen.genRand.Next(roomDatas.Count());
			roomData = roomDatas.ElementAt(roomId);
		}

		if (opening == OpeningType.Right) // Right-placed needs to be adjusted
		{
			x++;
		}

		return new PlacedRoom(roomData.Value, instance.PlaceRoom(roomData.Key, x, y, roomData.Value.OpeningLocation));
	}

	public static PlacedRoom PlaceRandomRoom(OpeningType opening, int x, int y)
	{
		RoomDatabase instance = ModContent.GetInstance<RoomDatabase>();
		IEnumerable<KeyValuePair<int, RoomData>> roomDatas = instance.DataByRoomIndex.Where(x => x.Value.Opening == opening);
		int roomId = WorldGen.genRand.Next(roomDatas.Count());
		KeyValuePair<int, RoomData> roomData = roomDatas.ElementAt(roomId);

		if (opening == OpeningType.Right) // Right-placed needs to be adjusted
		{
			x++;
		}

		return new PlacedRoom(roomData.Value, instance.PlaceRoom(roomData.Key, x, y, roomData.Value.OpeningLocation));
	}

	public void PlaceRoom(int id, int x, int y, Vector2 origin)
	{
		DataByRoomIndex[id].PlaceRoom(x, y, id, origin);
	}

	public Rectangle PlaceRoom(int id, int x, int y, Point origin)
	{
		return DataByRoomIndex[id].PlaceRoom(x, y, id, origin);
	}

	public void AddTimerInfo(List<EngageTimerInfo> info)
	{
		_timers.AddRange(info);
	}

	public override void Load()
	{
		DataByRoomIndex.Clear();
		DataByRoomIndex.Add(0, new RoomData(WireColor.Yellow, OpeningType.Right, new Point(44, 29), new Point(23, 39), null, [new EngageTimerInfo(new Point16(41, 5), 0)]));

		DataByRoomIndex.Add(1, new RoomData(WireColor.Blue, OpeningType.Left, new Point(0, 11), new Point(89, 56), 
			[new SpikeballInfo(new Point16(23, 49), 80, true, 0.05f), new(new Point16(38, 49), 80, true, 0.05f), new(new Point16(51, 49), 80, true, 0.05f), 
				new SpikeballInfo(new Point16(66, 49), 80, true, 0.05f)],
			[new EngageTimerInfo(new Point16(87, 5), 0), new EngageTimerInfo(new Point16(90, 11), 60)]));

		DataByRoomIndex.Add(2, new RoomData(WireColor.Green, OpeningType.Above, new Point(33, 0), new Point(33, 85),
			[new SpikeballInfo(new Point16(17, 61), 110, false), new(new Point16(51, 61), 110, false)],
			[new EngageTimerInfo(new Point16(18, 27), 0), new EngageTimerInfo(new Point16(20, 27), 45), new EngageTimerInfo(new Point16(22, 27), 90),
				new EngageTimerInfo(new Point16(24, 27), 135), 
				new EngageTimerInfo(new Point16(39, 34), 0), new EngageTimerInfo(new Point16(40, 34), 60), new EngageTimerInfo(new Point16(41, 34), 120)]));

		DataByRoomIndex.Add(3, new RoomData(WireColor.Red, OpeningType.Right, new Point(97, 13), new Point(93, 54),
			[new SpikeballInfo(new(32, 35), 90), new(new(55, 35), 90), new(new(78, 35), 90), new(new(32, 47), 90), new(new(55, 47), 90), new(new(78, 47), 90),
				new SpikeballInfo(new(76, 16), 90)],
			[new EngageTimerInfo(new(5, 7), 0), new(new(7, 9), 60), new(new(12, 49), 0)]));

		// This room's entrance is offset by 10 tiles to make sure it doesn't overlap the chasms by offsetting origin.
		DataByRoomIndex.Add(4, new RoomData(WireColor.Yellow, OpeningType.Above, new Point(24, 0), new Point(73, 72),
			[new SpikeballInfo(new(55, 39), 90, true, 0.04f), new(new(38, 39), 90), new(new(14, 39), 90)],
			[new EngageTimerInfo(new(68, 25), 0), new(new(69, 25), 90), new(new(65, 67), 0), new(new(66, 67), 60), new(new(67, 67), 120), 
				new EngageTimerInfo(new(68, 67), 180)]));

		DataByRoomIndex.Add(5, new RoomData(WireColor.Blue, OpeningType.Left, new Point(1, 8), new Point(14, 78),
			[new SpikeballInfo(new(27, 50), 90, true, 0.04f), new(new(27, 50), 180, false), new(new(27, 50), 270, true)], null));

		DataByRoomIndex.Add(6, new RoomData(WireColor.Green, OpeningType.Left, new Point(1, 44), new Point(6, 51), null,
			[new EngageTimerInfo(new(61, 33), 0), new(new(63, 35), 120), new(new(60, 23), 0), new(new(61, 23), 60), new(new(62, 23), 120), new(new(63, 23), 180),
				new EngageTimerInfo(new(60, 25), 240), new(new(61, 25), 300), new(new(62, 25), 360), new(new(63, 25), 420), new(new(60, 31), 720), new(new(61, 31), 780), 
				new EngageTimerInfo(new(62, 31), 840), new(new(63, 31), 900), new(new(52, 7), 0), new(new(53, 7), 75), new(new(54, 7), 150), new(new(55, 7), 225), 
				new(new(60, 44), 0)]));
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
