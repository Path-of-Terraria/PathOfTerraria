using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Subworlds.Passes;
using PathOfTerraria.Common.Subworlds.Passes.CaveSystemPasses;
using PathOfTerraria.Content.Items.Consumables.Maps;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds;

public class CaveRoom
{
	public Vector2 Position;
	public int Size;
	public int Connections;
	public List<CaveRoom> AllConnections = [];

	// public bool Spawned = false;

	public CaveRoom(Vector2 position, int size, int connections)
	{
		Position = position;
		Size = size;
		Connections = connections;
	}
}

/// <summary>
/// This is a world that can be manipulated for testing new world generations through the "newworld" command
/// </summary>
public class CaveSystemWorld : MappingWorld
{
	public override int Width => 500;
	public override int Height => 500;
	public override List<GenPass> Tasks => [
		new FillWorldPass(),
		new FillPositionsPass(),
		new SpawnAndBossPositionPass(),
		new RoomPositionPass(),
		new SpawnToBossPass(),
		new RemainingRoomLinkingPass(),
		new CaveClearingPass(),
		new SpawnMobsPass(),
	];

	// idk why im using tuples for everything, just too lazy to make a separate class/struct...
	public static List<Vector2> AvailablePositions = [];
	public static List<CaveRoom> Rooms = [];
	// <pos, size, connections>
	public static List<Tuple<Vector2, Vector2>> Lines = [];

	public static CaveRoom SpawnRoom => Rooms[0];
	public static CaveRoom BossRoom => Rooms[1];

	internal static CaveMap Map = null;

	public static void AddLine(int r1, int r2)
	{
		Rooms[r1].Connections++;
		Rooms[r2].Connections++;

		Rooms[r1].AllConnections.Add(Rooms[r2]);
		Rooms[r2].AllConnections.Add(Rooms[r1]);

		Lines.Add(new(Rooms[r1].Position, Rooms[r2].Position));
	}
	public static void AddLine(CaveRoom r1, CaveRoom r2)
	{
		r1.Connections++;
		r2.Connections++;

		r1.AllConnections.Add(r2);
		r2.AllConnections.Add(r1);

		Lines.Add(new(r1.Position, r2.Position));
	}
	public static void AddRoom(Vector2 position, int size)
	{
		Rooms.Add(new(position, size, 0));

		AvailablePositions = AvailablePositions.Where(point => point.Distance(position) > size + Map.ExtraRoomDist + Map.RoomSizeMax).ToList();
	}

	/*
	public override void Update()
	{
		foreach (Player player in Main.ActivePlayers)
		{
			foreach (CaveRoom room in Rooms.Where(r => !r.Spawned))
			{
				if (player.Center.Distance(room.Position) < room.Size + Main.screenWidth / 2f) // spawn condition needs to be re-evaluated at some point
				{
					room.Spawned = true;

					// spawn mobs
					// but if we want this to be neat, we might want to place mobs in the tunnels too...
				}
			}
		}
	}
	*/
}