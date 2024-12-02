using PathOfTerraria.Common.Systems;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using PathOfTerraria.Common.World.Generation;
using Terraria.DataStructures;
using Terraria.Localization;
using PathOfTerraria.Common.Subworlds.BossDomains.SkeleDomain;
using System.Linq;
using Terraria.Utilities;
using PathOfTerraria.Content.Projectiles.Utility;

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

	public override int Width => 900;
	public override int Height => 1000;
	public override (int time, bool isDay) ForceTime => ((int)Main.nightLength / 2, false);

	const int BaseTunnelDepth = 90;

	/// <summary>
	/// Per floor, the actuator info needed to wire rooms to the chasms.
	/// </summary>
	private readonly static Dictionary<int, FloorActuatorInfo> ActuatorInfoByFloor = [];

	/// <summary>
	/// Used to populate the randomly generated halls and falls with stuff.
	/// </summary>
	private readonly static HashSet<Point> CorridorTiles = [];

	/// <summary>
	/// Stops bone clusters from spawning in chasms.
	/// </summary>
	private readonly static HashSet<Point> ChasmTiles = [];

	/// <summary>
	/// Current floor.
	/// </summary>
	private static int Floor = 0;

	public override int[] WhitelistedCutTiles => [TileID.Cobweb];

	private readonly List<Point16> clearYPositions = [];

	public Rectangle Arena = Rectangle.Empty;
	public Point PortalLocation = Point.Zero;
	public Point WellBottom = Point.Zero;
	public bool BossSpawned = false;
	public bool ReadyToExit = false;
	
	private int clearY = 0;

	private readonly List<PlacedRoom> SpecialRooms = [];
	private readonly List<PlacedRoom> RoomsToWire = [];

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Surface", GenTerrain),
		new PassLegacy("Arena", SpawnArena),
		new PassLegacy("Tunnels", DigTunnels),
		new PassLegacy("Decor", AddDecor),
		new PassLegacy("Convert", DungeonConversion.Convert)];

	private void AddDecor(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.PopulatingWorld");

		HashSet<Point> blackList = [];

		foreach (PlacedRoom room in SpecialRooms)
		{
			for (int i = room.Area.X; i < room.Area.Right; ++i)
			{
				for (int j = room.Area.Y; j < room.Area.Bottom; ++j)
				{
					blackList.Add(new Point(i, j));
				}
			}

			continue;
		}

		foreach (Point point in CorridorTiles)
		{
			if (blackList.Contains(point))
			{
				continue;
			}

			if (WorldGen.genRand.NextBool(600) && !ChasmTiles.Contains(point))
			{
				WorldGen.TileRunner(point.X, point.Y, WorldGen.genRand.Next(5, 12), 8, TileID.BoneBlock);
			}
		}

		foreach (Point point in CorridorTiles)
		{
			if (blackList.Contains(point))
			{
				continue;
			}

			if (WorldGen.genRand.NextBool(180))
			{
				WorldGen.PlaceObject(point.X, point.Y, TileID.Painting3X3, true, WorldGen.genRand.Next(16, 18));
			}
			else if (WorldGen.genRand.NextBool(80))
			{
				WeightedRandom<int> styles = new(WorldGen.genRand);
				styles.Add(0, 1);
				styles.Add(4, 0.05f);
				WorldGen.PlaceObject(point.X, point.Y, TileID.HangingLanterns, true, styles);
			}
			else if (WorldGen.genRand.NextBool(150))
			{
				WorldGen.PlaceObject(point.X, point.Y, TileID.Chandeliers, true, 27);
			}
			else if (WorldGen.genRand.NextBool(60))
			{
				WorldGen.PlaceObject(point.X, point.Y, TileID.Statues, true, 46);
			}
			else if (WorldGen.genRand.NextBool(800))
			{
				WorldGen.PlaceObject(point.X, point.Y, TileID.BewitchingTable, true);
			}
			else if (WorldGen.genRand.NextBool(200))
			{
				WorldGen.PlaceObject(point.X, point.Y, TileID.GrandfatherClocks, true, 30);
			}
			else if (WorldGen.genRand.NextBool(180))
			{
				if (WorldGen.PlaceObject(point.X, point.Y, TileID.Bookcases, true, 1))
				{
					int type = WorldGen.genRand.NextBool(3) ? TileID.WaterCandle : TileID.Candles;
					int style = type == TileID.WaterCandle || WorldGen.genRand.NextBool(3) ? 0 : 1;
					WorldGen.PlaceObject(point.X + WorldGen.genRand.Next(-1, 2), point.Y - 4, type, true, style);
				}
			}
			else if (WorldGen.genRand.NextBool(180))
			{
				WeightedRandom<int> styles = new(WorldGen.genRand);
				styles.Add(2, 1);
				styles.Add(10, 1);
				styles.Add(11, 1);

				WorldGen.PlaceObject(point.X, point.Y, TileID.Banners, true, styles);
			}
			else if (WorldGen.genRand.NextBool(420) && !clearYPositions.Any(x => Math.Abs(x.X - point.X) < 10))
			{
				PlaceSpikes(point.X, point.Y);
			}
		}

		for (int i = 10; i < Width - 10; ++i)
		{
			for (int j = 50; j < 200; ++j)
			{
				OpenFlags flags = OpenExtensions.GetOpenings(i, j, false, false);

				if (flags == OpenFlags.None)
				{
					continue;
				}

				Tile tile = Main.tile[i, j];

				if (tile.HasTile && tile.TileType == TileID.Dirt)
				{
					tile.TileType = TileID.Grass;

					PlaceGrassStuff(i, j);
				}
			}
		}
	}

	private static void PlaceSpikes(int x, int y)
	{
		int width = WorldGen.genRand.Next(3, 8);
		int dir = WorldGen.genRand.NextBool() ? -1 : 1;

		while (!WorldGen.SolidOrSlopedTile(x, y))
		{
			y += dir;
		}

		for (int i = 0; i < width; ++i)
		{
			if (WorldGen.SolidOrSlopedTile(x + i, y + dir))
			{
				WorldGen.PlaceTile(x + i, y + dir, TileID.Spikes, true, true);
			}

			if (WorldGen.SolidOrSlopedTile(x + i, y + i % 2 * 2 * dir))
			{
				WorldGen.PlaceTile(x + i, y + i % 2 * 2 * dir, TileID.Spikes, true, true);
			}
		}
	}

	private static void PlaceGrassStuff(int i, int j)
	{
		if (!WorldGen.genRand.NextBool(3))
		{
			WorldGen.PlaceTile(i, j - 1, TileID.Plants);
		}
		else if (WorldGen.genRand.NextBool(6))
		{
			WorldGen.PlaceTile(i, j - 1, TileID.Saplings);

			if (!WorldGen.GrowTree(i, j - 1))
			{
				WorldGen.KillTile(i, j - 1);
			}
		}
		else if (WorldGen.genRand.NextBool(4))
		{
			WorldGen.PlaceSmallPile(i, j - 1, WorldGen.genRand.Next(10), 0);
		}
	}

	private void DigTunnels(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Tunnels");

		WellBottom.X = DigChasm(WellBottom.Y - 1, WellBottom.Y + BaseTunnelDepth, WellBottom.X, 4, 6);
		Point secondFloorStart = GenerateFirstFloor();
		Point thirdFloorStart = GenerateSecondFloor(secondFloorStart.X, secondFloorStart.Y);
		GenerateThirdFloor(thirdFloorStart.X, thirdFloorStart.Y);

		ActuatorInfoByFloor.Clear();
	}

	private static int DigChasm(int startY, int endY, float baseX, int wallDepth, int tunnelWidth, bool spawnActuatedWall = false, 
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
			for (int x = (int)baseX - halfWidth; x < baseX + halfWidth; ++x)
			{
				Tile tile = Main.tile[x, y];

				tile.ClearEverything();

				if (x < baseX - halfWidth + wallDepth || x >= baseX + halfWidth - wallDepth)
				{
					tile.TileType = (ushort)tileType;
					tile.HasTile = true;
				}
				else
				{
					if (spawnActuatedWall && pregeneratedActuatedWallYs.TryGetValue(y, out int value))
					{
						tile.TileType = (ushort)tileType;
						tile.HasTile = true;
						tile.HasActuator = true;
						tile.IsActuated = false;

						if (!ActuatorInfoByFloor.ContainsKey(Floor))
						{
							ActuatorInfoByFloor.Add(Floor, new FloorActuatorInfo(Floor + 2));
						}

						ActuatorInfoByFloor[Floor].AddActuatedTile(value, new Point(x, y));
					}
					
					if ((!spawnActuatedWall || y > startY + actuatedWallCount * 4) && y % 10 == 0)
					{
						tile.TileType = TileID.Platforms;
						tile.HasTile = true;
						tile.TileFrameY = (short)((short)(tileType == TileID.GrayBrick ? 43 : 6) * 18);

						if (!WorldGen.genRand.NextBool(6) && tileType != TileID.GrayBrick)
						{
							int type = !WorldGen.genRand.NextBool(15) ? TileID.Books : 
								WorldGen.genRand.NextBool(3) ? TileID.WaterCandle : TileID.Candles;
							int style = type != TileID.Candles || WorldGen.genRand.NextBool(3) ? 0 : 1;

							if (type == TileID.Books)
							{
								style = WorldGen.genRand.Next(5);

								if (WorldGen.genRand.NextBool(60))
								{
									style = 5;
								}
							}

							WorldGen.PlaceObject(x, y - 1, type, true, style);
						}
					}
					else if (tileType != TileID.GrayBrick)
					{
						ChasmTiles.Add(new Point(x, y)); // Chasm tiles stop bone clusters from blocking passages
						CorridorTiles.Add(new Point(x, y));
					}
				}

				tile.WallType = (ushort)wallType;
			}

			baseX += (int)(noise.GetNoise(0, y) * 2) / 2f;

			// Cap it so the chasms don't let the rooms go off world
			if (baseX > 500)
			{
				baseX = 500;
			}
			else if (baseX < 300)
			{
				baseX = 300;
			}
		}

		return (int)baseX;
	}

	private Point GenerateFirstFloor()
	{
		Floor = 0;
		List<WireColor> usedColors = [];

		int roomHeight = WorldGen.genRand.Next(12, 16);
		CreatePlainRoom(WellBottom.X, WellBottom.Y + BaseTunnelDepth, WorldGen.genRand.Next(17, 23), roomHeight, true, true);

		clearYPositions.Add(new(WellBottom.X, WellBottom.Y + BaseTunnelDepth));

		int corridorEnd = WellBottom.X - WorldGen.genRand.Next(50, 80);
		int corridorEndY = WellBottom.Y + BaseTunnelDepth + 2 + WorldGen.genRand.Next(-6, 6);

		RunCorridor(WellBottom.X, WellBottom.Y + BaseTunnelDepth + 2, corridorEnd, corridorEndY);
		AddRoom(RoomDatabase.PlaceRandomRoom(OpeningType.Right, corridorEnd, corridorEndY, usedColors));

		corridorEnd = WellBottom.X + WorldGen.genRand.Next(50, 80);
		corridorEndY = WellBottom.Y + BaseTunnelDepth + 2 + WorldGen.genRand.Next(-6, 6);

		RunCorridor(WellBottom.X, WellBottom.Y + BaseTunnelDepth + 2, corridorEnd, corridorEndY);
		AddRoom(RoomDatabase.PlaceRandomRoom(OpeningType.Left, corridorEnd, corridorEndY, usedColors));

		int chasmBottom = WellBottom.Y + BaseTunnelDepth + 2 + 180;
		int lastX = DigChasm(WellBottom.Y + BaseTunnelDepth + 2 + roomHeight / 2 - 2, chasmBottom, WellBottom.X, 4, 6, true, TileID.BlueDungeonBrick, WallID.BlueDungeonUnsafe, 2);

		WireRoomsToChasms(ActuatorInfoByFloor[Floor], RoomsToWire);
		return new Point(lastX, chasmBottom);
	}

	private Point GenerateSecondFloor(int x, int y)
	{
		Floor = 1;
		List<WireColor> usedColors = [];

		int roomHeight = WorldGen.genRand.Next(16, 21);
		CreatePlainRoom(x, y, WorldGen.genRand.Next(23, 34), roomHeight, true, true);

		clearYPositions.Add(new(x, y));

		bool left = WorldGen.genRand.NextBool();
		int corridorEnd = x - (left ? WorldGen.genRand.Next(150, 180) : WorldGen.genRand.Next(50, 80));

		RunCorridor(x, y, corridorEnd, y);

		if (left)
		{
			AddRoom(RoomDatabase.PlaceRandomRoom(OpeningType.Above, (corridorEnd + x) / 2, y + 4, usedColors));
		}

		AddRoom(RoomDatabase.PlaceRandomRoom(OpeningType.Right, corridorEnd, y, usedColors));

		corridorEnd = x + (!left ? WorldGen.genRand.Next(150, 180) : WorldGen.genRand.Next(50, 80));

		RunCorridor(x, y, corridorEnd, y);
		AddRoom(RoomDatabase.PlaceRandomRoom(OpeningType.Left, corridorEnd, y, usedColors));

		if (!left)
		{
			AddRoom(RoomDatabase.PlaceRandomRoom(OpeningType.Above, (corridorEnd + x) / 2, y + 3, usedColors));
		}

		int lastX = DigChasm(y + roomHeight / 2 - 2, y + 220, x, 4, 6, true, TileID.BlueDungeonBrick, WallID.BlueDungeonUnsafe, 3);
		WireRoomsToChasms(ActuatorInfoByFloor[1], RoomsToWire);
		return new Point(lastX, y + 220);
	}

	private void GenerateThirdFloor(int x, int y)
	{
		Floor = 2;
		List<WireColor> usedColors = [];

		int roomHeight = WorldGen.genRand.Next(16, 21);
		CreatePlainRoom(x, y, WorldGen.genRand.Next(23, 34), roomHeight, true, true);
		
		clearYPositions.Add(new(x, y));

		int corridorEnd = x - WorldGen.genRand.Next(140, 170);
		bool left = WorldGen.genRand.NextBool();

		RunCorridor(x, y, corridorEnd, y);

		AddRoom(RoomDatabase.PlaceRandomRoom(OpeningType.Above, (corridorEnd + x) / 2, y + 4, usedColors));
		AddRoom(RoomDatabase.PlaceRandomRoom(OpeningType.Right, corridorEnd, y, usedColors));

		corridorEnd = x + WorldGen.genRand.Next(140, 170);

		RunCorridor(x, y, corridorEnd, y);
		AddRoom(RoomDatabase.PlaceRandomRoom(OpeningType.Left, corridorEnd, y, usedColors));
		AddRoom(RoomDatabase.PlaceRandomRoom(OpeningType.Above, (corridorEnd + x) / 2, y + 4, usedColors));

		int lastX = DigChasm(y + roomHeight / 2 - 2, y + 120, x, 4, 6, true, TileID.BlueDungeonBrick, WallID.BlueDungeonUnsafe, 4);
		WireRoomsToChasms(ActuatorInfoByFloor[2], RoomsToWire);
		CreatePlainRoom(lastX, y + 120, WorldGen.genRand.Next(23, 34), roomHeight, true, true);

		PortalLocation = new Point(lastX, y + 124);
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
			var wirePosition = new Point(loc.X + data.WireConnection.X - 1, loc.Y + data.WireConnection.Y - 1);
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
					CorridorTiles.Add(new Point(i, j));
					continue;
				}

				tile.TileType = TileID.BlueDungeonBrick;
			}
		}
	}

	private static void CreatePlainRoom(int x, int y, int width, int height, bool dontPlace, bool addToCorridorTiles = false)
	{
		ShapeData shapeData = new();
		x -= width / 2;
		y -= height / 2;

		WorldUtils.Gen(new Point(x, y), new Shapes.Rectangle(width, height), Actions.Chain( // Clear & add walls
			new Actions.Clear().Output(shapeData),
			new Actions.PlaceWall(WallID.BlueDungeon)
		));

		if (addToCorridorTiles)
		{
			foreach (Point16 point in shapeData.GetData())
			{
				CorridorTiles.Add(new Point(point.X + x, point.Y + y));
			}
		}

		for (int i = x - 6; i <= x + width + 6; ++i)
		{
			for (int j = y - 6; j <= y + height + 6; ++j)
			{
				if (i < x || j < y || i >= x + width || j >= y + height)
				{
					Tile tile = Main.tile[i, j];

					if (dontPlace && !WorldGen.SolidOrSlopedTile(i, j))
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
		SpecialRooms.Clear();
		CorridorTiles.Clear();
		ChasmTiles.Clear();

		Main.spawnTileX = WorldGen.genRand.NextBool() ? 180 : Main.maxTilesX - 180;
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
				Tile tile = Main.tile[x, y];
				tile.TileType = y > 400 + noiseOffset ? TileID.Stone : TileID.Dirt;
				tile.HasTile = true;

				if (y > useY + 4)
				{
					tile.WallType = WallID.Stone;
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
	}

	public override void Update()
	{
		Wiring.UpdateMech();

		bool hasProj = false;

		foreach (Projectile proj in Main.ActiveProjectiles)
		{
			if (proj.type == ModContent.ProjectileType<Teleportal>())
			{
				hasProj = true;
			}
		}

		if (!hasProj)
		{
			int type = ModContent.ProjectileType<Teleportal>();
			Vector2 position = PortalLocation.ToWorldCoordinates();
			Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), position, Vector2.Zero, type, 0, 0, -1, Width / 2 * 16, (Height - 140) * 16);
		}

		bool allInArena = Main.CurrentFrameFlags.ActivePlayersCount > 0;

		foreach (Player player in Main.ActivePlayers)
		{
			if (allInArena && !Arena.Intersects(player.Hitbox))
			{
				allInArena = false;
			}

			if (clearYPositions.Count > 0 && player.position.Y / 16 > clearY)
			{
				clearY = (int)player.position.Y / 16;

				if (clearYPositions.Any(x => x.Y < clearY && x.ToWorldCoordinates().DistanceSQ(player.Center) < 400 * 400))
				{
					Main.spawnTileX = (int)player.Center.X / 16;
					Main.spawnTileY = clearY;

					clearYPositions.RemoveAll(x => x.Y < clearY && x.ToWorldCoordinates().DistanceSQ(player.Center) < 400 * 400);

					if (Main.netMode != NetmodeID.SinglePlayer)
					{
						NetMessage.SendData(MessageID.WorldData);
					}
				}
			}
		}

		if (!BossSpawned && allInArena)
		{
			NPC.NewNPC(Entity.GetSource_NaturalSpawn(), Arena.Center.X, Arena.Center.Y, NPCID.SkeletronHead, 1);
			BossSpawned = true;

			Main.spawnTileX = Arena.Center.X / 16;
			Main.spawnTileY = Arena.Center.Y / 16;

			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				NetMessage.SendData(MessageID.WorldData);
			}
		}

		if (BossSpawned && !NPC.AnyNPCs(NPCID.SkeletronHead) && !ReadyToExit)
		{
			Vector2 pos = Arena.Center() + new Vector2(0, 240);
			Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), pos, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);

			BossTracker.CachedBossesDowned.Add(NPCID.SkeletronHead);
			ReadyToExit = true;
		}
	}
}
