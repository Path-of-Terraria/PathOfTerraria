using PathOfTerraria.Content.Projectiles;
using PathOfTerraria.Common.Systems;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.Systems.DisableBuilding;
using Terraria.DataStructures;
using Terraria.Localization;
using PathOfTerraria.Common.Subworlds.BossDomains.SkeleDomain;
using System.Linq;

namespace PathOfTerraria.Common.Subworlds.BossDomains;

public class SkeletronDomain : BossDomainSubworld
{
	public class FloorActuatorInfo(int actuatedWallCount)
	{
		public readonly int ActuatedWallCount = actuatedWallCount;

		public Dictionary<int, HashSet<Point>> ActuatedTilesByWall = [];

		public void AddActuatedTile(int wall, Point position)
		{
			if (ActuatedTilesByWall.TryGetValue(wall, out HashSet<Point> tiles))
			{
				tiles.Add(position);
			}
			else
			{
				ActuatedTilesByWall.Add(wall, [position]);
			}
		}
	}

	public override int Width => 800;
	public override int Height => 1000;

	const int BaseTunnelDepth = 90;

	private readonly static Dictionary<int, FloorActuatorInfo> ActuatorInfoByFloor = [];

	public static int Floor = 0;

	public override int[] WhitelistedCutTiles => [TileID.Cobweb];

	public Rectangle Arena = Rectangle.Empty;
	public Point WellBottom = Point.Zero;
	public bool BossSpawned = false;
	public bool ReadyToExit = false;

	private readonly List<PlacedRoom> SpecialRooms = [];
	private readonly List<PlacedRoom> RoomsToWire = [];

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Surface", GenTerrain),
		new PassLegacy("Arena", SpawnArena),
		new PassLegacy("Tunnels", DigTunnels)];

	private void DigTunnels(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Tunnels");

		WellBottom.X = DigChasm(WellBottom.Y - 1, WellBottom.Y + BaseTunnelDepth, WellBottom.X, 4, 6);
		Point secondFloorStart = GenerateFirstFloor();
		Point thirdFloorStart = GenerateSecondFloor(secondFloorStart.X, secondFloorStart.Y);
		GenerateThirdFloor(thirdFloorStart.X, thirdFloorStart.Y);

		ActuatorInfoByFloor.Clear();
	}

	private static int DigChasm(int startY, int endY, int baseX, int wallDepth, int tunnelWidth, bool spawnActuatedWall = false, 
		int tileType = TileID.GrayBrick, int wallType = WallID.GrayBrick, int actuatedWallCount = 2)
	{
		FastNoiseLite noise = GetGenNoise();
		int halfWidth = wallDepth + tunnelWidth / 2;

		Dictionary<int, int> pregeneratedActuatedWallYs = [];

		if (spawnActuatedWall)
		{
			for (int i = 0; i < actuatedWallCount; ++i)
			{
				pregeneratedActuatedWallYs.Add(startY + (i + 1) * 4, i);
			}
		}

		for (int y = startY; y < endY; ++y)
		{
			for (int x = baseX - halfWidth; x < baseX + halfWidth; ++x)
			{
				Tile tile = Main.tile[x, y];

				tile.ClearEverything();

				if (x < baseX - halfWidth + wallDepth || x >= baseX + halfWidth - wallDepth)
				{
					tile.TileType = (ushort)tileType;
					tile.HasTile = true;
				}
				else if (spawnActuatedWall && pregeneratedActuatedWallYs.ContainsKey(y))
				{
					tile.TileType = (ushort)tileType;
					tile.HasTile = true;
					tile.HasActuator = true;
					tile.IsActuated = false;

					if (!ActuatorInfoByFloor.ContainsKey(Floor))
					{
						ActuatorInfoByFloor.Add(Floor, new FloorActuatorInfo(Floor + 2));
					}

					ActuatorInfoByFloor[Floor].AddActuatedTile(pregeneratedActuatedWallYs[y], new Point(x, y));
				}

				tile.WallType = (ushort)wallType;
			}

			baseX += (int)(noise.GetNoise(0, y) * 2);
		}

		return baseX;
	}

	private Point GenerateFirstFloor()
	{
		Floor = 0;

		int roomHeight = WorldGen.genRand.Next(12, 16);
		CreatePlainRoom(WellBottom.X, WellBottom.Y + BaseTunnelDepth, WorldGen.genRand.Next(17, 23), roomHeight, true);

		int corridorEnd = WellBottom.X - WorldGen.genRand.Next(50, 80);
		int corridorEndY = WellBottom.Y + BaseTunnelDepth + 2 + WorldGen.genRand.Next(-6, 6);

		RunCorridor(WellBottom.X, WellBottom.Y + BaseTunnelDepth + 2, corridorEnd, corridorEndY);
		AddRoom(RoomDatabase.PlaceRandomRoom(OpeningType.Right, corridorEnd, corridorEndY));

		corridorEnd = WellBottom.X + WorldGen.genRand.Next(50, 80);
		corridorEndY = WellBottom.Y + BaseTunnelDepth + 2 + WorldGen.genRand.Next(-6, 6);

		RunCorridor(WellBottom.X, WellBottom.Y + BaseTunnelDepth + 2, corridorEnd, corridorEndY);
		AddRoom(RoomDatabase.PlaceRandomRoom(OpeningType.Left, corridorEnd, corridorEndY));

		int chasmBottom = WellBottom.Y + BaseTunnelDepth + 2 + 120;
		int lastX = DigChasm(WellBottom.Y + BaseTunnelDepth + 2 + roomHeight / 2 - 1, chasmBottom, WellBottom.X, 4, 6, true, TileID.BlueDungeonBrick, WallID.BlueDungeonUnsafe, 2);

		WireRoomsToChasms(ActuatorInfoByFloor[Floor], RoomsToWire);
		return new Point(lastX, chasmBottom);
	}

	private Point GenerateSecondFloor(int x, int y)
	{
		Floor = 1;

		int roomHeight = WorldGen.genRand.Next(16, 21);
		CreatePlainRoom(x, y, WorldGen.genRand.Next(23, 34), roomHeight, true);

		int corridorEnd = x - WorldGen.genRand.Next(150, 180);
		bool left = WorldGen.genRand.NextBool();

		RunCorridor(x, y, corridorEnd, y);

		if (left)
		{
			AddRoom(RoomDatabase.PlaceRandomRoom(OpeningType.Above, (corridorEnd + x) / 2, y + 4));
		}

		AddRoom(RoomDatabase.PlaceRandomRoom(OpeningType.Right, corridorEnd, y));

		corridorEnd = x + WorldGen.genRand.Next(150, 180);

		RunCorridor(x, y, corridorEnd, y);
		AddRoom(RoomDatabase.PlaceRandomRoom(OpeningType.Left, corridorEnd, y));

		if (!left)
		{
			AddRoom(RoomDatabase.PlaceRandomRoom(OpeningType.Above, (corridorEnd + x) / 2, y + 3));
		}

		int lastX = DigChasm(y + roomHeight / 2 - 1, y + 120, x, 4, 6, true, TileID.BlueDungeonBrick, WallID.BlueDungeonUnsafe, 3);
		WireRoomsToChasms(ActuatorInfoByFloor[1], RoomsToWire);
		return new Point(lastX, y + BaseTunnelDepth + 2 + 120);
	}

	private Point GenerateThirdFloor(int x, int y)
	{
		Floor = 2;

		int roomHeight = WorldGen.genRand.Next(16, 21);
		CreatePlainRoom(x, y, WorldGen.genRand.Next(23, 34), roomHeight, true);

		int corridorEnd = x - WorldGen.genRand.Next(150, 180);
		bool left = WorldGen.genRand.NextBool();

		RunCorridor(x, y, corridorEnd, y);

		AddRoom(RoomDatabase.PlaceRandomRoom(OpeningType.Above, (corridorEnd + x) / 2, y + 4));
		AddRoom(RoomDatabase.PlaceRandomRoom(OpeningType.Right, corridorEnd, y));

		corridorEnd = x + WorldGen.genRand.Next(150, 180);

		RunCorridor(x, y, corridorEnd, y);
		AddRoom(RoomDatabase.PlaceRandomRoom(OpeningType.Left, corridorEnd, y));
		AddRoom(RoomDatabase.PlaceRandomRoom(OpeningType.Above, (corridorEnd + x) / 2, y + 3));

		int lastX = DigChasm(y + roomHeight / 2 - 1, y + 120, x, 4, 6, true, TileID.BlueDungeonBrick, WallID.BlueDungeonUnsafe, 4);
		WireRoomsToChasms(ActuatorInfoByFloor[1], RoomsToWire);
		return new Point(lastX, y + BaseTunnelDepth + 2 + 120);
	}

	private void AddRoom(PlacedRoom room)
	{
		SpecialRooms.Add(room);
		RoomsToWire.Add(room);
	}

	private void WireRoomsToChasms(FloorActuatorInfo floorActuatorInfo, List<PlacedRoom> roomsToWire)
	{
		if (floorActuatorInfo.ActuatedWallCount != roomsToWire.Count)
		{
			throw new Exception($"Wall count doesn't match room count. Expected: {floorActuatorInfo.ActuatedWallCount}, Room Count: {roomsToWire.Count}");
		}

		for (int i = 0; i < floorActuatorInfo.ActuatedWallCount; i++)
		{
			RoomData data = roomsToWire[i].Data;

			foreach (Point position in floorActuatorInfo.ActuatedTilesByWall[i])
			{
				Tile tile = Main.tile[position];

				SetWireOnTile(data, tile);
			}

			Point loc = roomsToWire[i].Area.Location;
			Point wirePosition = new Point(loc.X + data.WireConnection.X - 1, loc.Y + data.WireConnection.Y - 1);
			Point point = floorActuatorInfo.ActuatedTilesByWall[i].First();

			SetWireOnTile(data, Main.tile[wirePosition.X + 1, wirePosition.Y]);

			int xDir = Math.Sign(point.X - wirePosition.X);

			while (wirePosition.X != point.X)
			{
				SetWireOnTile(data, Main.tile[wirePosition]);

				wirePosition.X += xDir;

				if (SpecialRooms.Any(x => x.Area.Contains(wirePosition)))
				{
					wirePosition.X -= xDir;
					wirePosition.Y++;
				}
			}

			int yDir = Math.Sign(point.Y - wirePosition.Y);

			while (wirePosition.Y != point.Y)
			{
				SetWireOnTile(data, Main.tile[wirePosition]);

				wirePosition.Y += yDir;
			}
		}

		roomsToWire.Clear();
	}

	private static void SetWireOnTile(RoomData data, Tile tile)
	{
		switch (data.Wire)
		{
			case WireColor.Red:
				tile.RedWire = true;
				break;

			case WireColor.Yellow:
				tile.YellowWire = true;
				break;

			case WireColor.Blue:
				tile.BlueWire = true;
				break;

			case WireColor.Green:
				tile.GreenWire = true;
				break;

			default:
				throw new Exception("Invalid wire type.");
		}
	}

	private static void RunCorridor(int x, int y, int endX, int endY)
	{
		int dif = Math.Abs(endX - x);
		int sign = Math.Sign(endX - x);

		for (int i = x; i != endX; i += sign) 
		{
			int baseY = (int)MathHelper.Lerp(y, endY, MathF.Abs(x - i) / dif);

			for (int j = baseY - 7; j < baseY + 8; j++)
			{
				Tile tile = Main.tile[i, j];
				tile.WallType = WallID.BlueDungeonUnsafe;

				if (j > baseY - 4 && j < baseY + 4)
				{
					tile.ClearTile();
					continue;
				}

				tile.TileType = TileID.BlueDungeonBrick;
			}
		}
	}

	private static void CreatePlainRoom(int x, int y, int width, int height, bool dontPlace)
	{
		ShapeData shapeData = new();
		x -= width / 2;
		y -= height / 2;

		WorldUtils.Gen(new Point(x, y), new Shapes.Rectangle(width, height), Actions.Chain( // Clear & add walls
			new Actions.Clear().Output(shapeData),
			new Actions.PlaceWall(WallID.BlueDungeon)
		));

		for (int i = x - 6; i <= x + width + 6; ++i)
		{
			for (int j = y - 6; j <= y + height + 6; ++j)
			{
				if (i < x || j < y || i >= x + width || j >= y + height)
				{
					Tile tile = Main.tile[i, j];

					if (dontPlace && !tile.HasTile)
					{
						continue;
					}

					tile.HasTile = true;
					tile.TileType = TileID.BlueDungeonBrick;
				}
			}
		}
	}

	private void SpawnArena(GenerationProgress progress, GameConfiguration configuration)
	{
		int y = 100;

		while (!Main.tile[Width / 2, y].HasTile)
		{
			y++;
		}

		StructureTools.PlaceByOrigin("Assets/Structures/SkeletronDomain/SkeletronWell", new Point16(Width / 2, y + 3), new Vector2(0.5f, 1));
		WellBottom = new Point(Width / 2, y + 3);

		Point16 size = Point16.Zero;
		StructureHelper.Generator.GetDimensions("Assets/Structures/SkeletronArena", Mod, ref size);
		var position = new Point16(Width / 2 - size.X / 2, Height - 150 - size.Y / 2);
		StructureHelper.Generator.GenerateStructure("Assets/Structures/SkeletronArena", position, Mod);

		Arena = new Rectangle(position.X * 16, position.Y * 16, size.X * 16, size.Y * 16);
	}

	private void GenTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		Main.spawnTileX = WorldGen.genRand.NextBool() ? 80 : Main.maxTilesX - 80;
		Main.spawnTileY = 110;
		Main.worldSurface = 230;
		Main.rockLayer = 299;

		float baseY = 120;

		FastNoiseLite noise = GetGenNoise();
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");

		for (int x = 0; x < Main.maxTilesX; ++x)
		{
			float noiseOffset = noise.GetNoise(x, 0) * 3;
			float useY = baseY + noiseOffset;

			for (int y = (int)useY; y < Main.maxTilesY; ++y)
			{
				WorldGen.PlaceTile(x, y, y > 400 + noiseOffset ? TileID.Stone : TileID.Dirt);

				if (y > useY + 4)
				{
					WorldGen.PlaceWall(x, y, WallID.Stone, true);
				}
			}

			progress.Value = (float)x / Main.maxTilesX;
		}

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Tunnels");
		progress.Value = 0;
	}

	private static FastNoiseLite GetGenNoise()
	{
		var noise = new FastNoiseLite(WorldGen._genRandSeed);
		noise.SetFrequency(0.02f);
		return noise;
	}

	public override void OnEnter()
	{
		BossSpawned = false;
		ReadyToExit = false;

		Main.dayTime = false;
		Main.time = Main.nightLength / 2;
	}

	public override void Update()
	{
		Main.dayTime = false;
		Main.time = Main.nightLength / 2;
		Wiring.UpdateMech();

		bool allInArena = true;

		foreach (Player player in Main.ActivePlayers)
		{
			player.GetModPlayer<StopBuildingPlayer>().ConstantStopBuilding = true;

			if (allInArena && !Arena.Intersects(player.Hitbox))
			{
				allInArena = false;
			}
		}

		if (!BossSpawned && allInArena)
		{
			NPC.NewNPC(Entity.GetSource_NaturalSpawn(), Arena.Center.X, Arena.Center.Y, NPCID.SkeletronHead, 1);
			BossSpawned = true;
		}

		if (BossSpawned && !NPC.AnyNPCs(NPCID.SkeletronHead) && !ReadyToExit)
		{
			Vector2 pos = Arena.Center() + new Vector2(0, 240);
			Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), pos, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);

			BossTracker.CachedBossesDowned.Add(NPCID.EaterofWorldsHead);
			ReadyToExit = true;
		}
	}
}
