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
			[new SpikeballInfo(new Point16(21, 47), 80, true, 0.05f), new(new Point16(36, 47), 80, true, 0.05f), new(new Point16(49, 47), 80, true, 0.05f), 
				new SpikeballInfo(new Point16(64, 47), 80, true, 0.05f)],
			[new EngageTimerInfo(new Point16(87, 5), 0), new EngageTimerInfo(new Point16(90, 11), 60)]));

		DataByRoomIndex.Add(2, new RoomData(WireColor.Green, OpeningType.Above, new Point(33, 0), new Point(33, 85),
			[new SpikeballInfo(new Point16(15, 59), 110, false), new(new Point16(49, 59), 110, false)],
			[new EngageTimerInfo(new Point16(18, 27), 0), new EngageTimerInfo(new Point16(20, 27), 45), new EngageTimerInfo(new Point16(22, 27), 90),
				new EngageTimerInfo(new Point16(24, 27), 135), 
				new EngageTimerInfo(new Point16(39, 34), 0), new EngageTimerInfo(new Point16(40, 34), 60), new EngageTimerInfo(new Point16(41, 34), 120)]));

		DataByRoomIndex.Add(3, new RoomData(WireColor.Red, OpeningType.Right, new Point(97, 13), new Point(93, 54),
			[new SpikeballInfo(new(30, 33), 90), new(new(53, 33), 90), new(new(76, 33), 90), new(new(30, 45), 90), new(new(53, 45), 90), new(new(76, 45), 90),
				new SpikeballInfo(new(74, 14), 90)],
			[new EngageTimerInfo(new(5, 7), 0), new(new(7, 9), 60), new(new(12, 49), 0)]));

		DataByRoomIndex.Add(4, new RoomData(WireColor.Yellow, OpeningType.Above, new Point(14, 0), new Point(73, 72),
			[new SpikeballInfo(new(53, 37), 90, true, 0.04f), new(new(36, 37), 90), new(new(12, 37), 90)],
			[new EngageTimerInfo(new(68, 25), 0), new(new(69, 25), 90), new(new(65, 67), 0), new(new(66, 67), 60), new(new(67, 67), 120), 
				new EngageTimerInfo(new(68, 67), 180)]));
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
