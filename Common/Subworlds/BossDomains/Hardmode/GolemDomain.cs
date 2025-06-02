using PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode.SkeleDomain;
using PathOfTerraria.Common.Subworlds.Tools;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.World.Generation.Tools;
using PathOfTerraria.Content.Projectiles.Utility;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;

internal class GolemDomain : BossDomainSubworld
{
	const int RoomCountPerTemple = 3;

	private record PairingRoom(PlacedRoom Room, bool Exit, int PairingNumber);

	public override int Width => 600;
	public override int Height => 1100;
	public override (int time, bool isDay) ForceTime => ((int)Main.dayLength / 2, true);

	private static readonly List<PairingRoom> Rooms = [];

	private static Rectangle Arena = Rectangle.Empty;
	private static bool BossSpawned = false;
	private static bool ExitSpawned = false;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Terrain", GenTerrain),
		new PassLegacy("Temples", Temples),
		new PassLegacy("Decor", Decor)];

	public override void Load()
	{
		On_Player.Teleport += AddGolemTeleporting;
	}

	private void AddGolemTeleporting(On_Player.orig_Teleport orig, Player self, Vector2 newPos, int Style, int extraInfo)
	{
		orig(self, newPos, Style, extraInfo);

		if (SubworldSystem.Current is GolemDomain)
		{
			Main.spawnTileX = (int)newPos.X / 16;
			Main.spawnTileY = (int)newPos.Y / 16;

			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				NetMessage.SendData(MessageID.WorldData);
			}
		}
	}

	private void Temples(GenerationProgress progress, GameConfiguration configuration)
	{
		const int Seperation = 48;

		Rooms.Clear();
		bool start = WorldGen.genRand.NextBool();
		List<(Point, Point)> tunnels = [];

		SpawnTemple(Width / 2 - Seperation, FindTileBelow(Width / 2 - Seperation, 100), start, tunnels);
		SpawnTemple(Width / 2 + Seperation, FindTileBelow(Width / 2 + Seperation, 100), !start, tunnels);
		GenerateTunnels(tunnels);
		PairTeleporters();
	}

	private static void PairTeleporters()
	{
		for (int i = 0; i < RoomCountPerTemple + 3; ++i)
		{
			PairingRoom exit = Rooms.Find(x => x.Exit && x.PairingNumber == i);
			PairingRoom entrance = Rooms.Find(x => !x.Exit && x.PairingNumber == i);

			Point exitWirePos = exit.Room.Data.WireConnection;
			Point pos = new(exit.Room.Area.X + exitWirePos.X, exit.Room.Area.Y + exitWirePos.Y - 1);

			Point entranceWirePos = entrance.Room.Data.WireConnection;
			Point target = new(entrance.Room.Area.X + entranceWirePos.X, entrance.Room.Area.Y + entranceWirePos.Y - 1);

			while (pos != target)
			{
				Tile tile = Main.tile[pos];
				tile.YellowWire = true;

				bool inStructure = GenVars.structures.CanPlace(new Rectangle(pos.X, pos.Y, 1, 1), 1);

				if (!inStructure || pos.Y <= target.Y)
				{
					int dir = pos.Y <= target.Y ? Math.Sign(target.X - pos.X) : 1;

					if (dir == 0)
					{
						pos.Y++;
					}
					else
					{
						pos.X += dir;
					}
				}
				else
				{
					pos.Y--;
				}
			}

			Tile finalTile = Main.tile[pos];
			finalTile.YellowWire = true;
		}
	}

	private static void GenerateTunnels(List<(Point, Point)> tunnels)
	{
		foreach ((Point exit, Point entrance) in tunnels)
		{
			var vExit = exit.ToVector2();
			var vEntrance = entrance.ToVector2();
			Vector2[] tunnel = Tunnel.GeneratePoints([vExit, Vector2.Lerp(vExit, vEntrance, 0.5f + WorldGen.genRand.NextFloat(-0.1f, 0.1f))
				- new Vector2(WorldGen.genRand.NextFloat(-32, 32), 0), vEntrance], 12, 1, 0.2f);
			HashSet<Point16> clearTiles = [];
			HashSet<Point16> allTiles = [];

			foreach (Vector2 pos in tunnel)
			{
				GatherTunnelData(pos, clearTiles, allTiles);
			}

			foreach (Point16 item in allTiles)
			{
				Tile tile = Main.tile[item];

				if (clearTiles.Contains(item))
				{
					tile.HasTile = false;
				}
				else
				{
					tile.HasTile = true;
					tile.TileType = TileID.LihzahrdBrick;
				}

				tile.WallType = WallID.LihzahrdBrickUnsafe;
			}
		}
	}

	private static void GatherTunnelData(Vector2 pos, HashSet<Point16> clearTiles, HashSet<Point16> allTiles)
	{
		var center = pos.ToPoint();

		for (int i = -8; i < 9; ++i)
		{
			for (int j = -6; j < 7; ++j)
			{
				var newPos = new Point16(center.X + i, center.Y + j);

				if (clearTiles.Contains(newPos) || !GenVars.structures.CanPlace(new Rectangle(newPos.X, newPos.Y, 1, 1)))
				{
					continue;
				}

				allTiles.Add(newPos);

				if (i > -5 && i < 5 && j > -4 && j < 4)
				{
					clearTiles.Add(newPos);
				}
			}
		}
	}

	private static void SpawnTemple(int x, int y, bool start, List<(Point, Point)> tunnels)
	{
		Point exit = Point.Zero;

		if (start)
		{
			string structure = "Assets/Structures/GolemDomain/Entrance_" + WorldGen.genRand.Next(3);
			Point16 size = StructureTools.GetSize(structure);
			Point16 pos = StructureTools.PlaceByOrigin(structure, new(x, y + 8), new Vector2(0.5f, 1));
			GenVars.structures.AddProtectedStructure(new Rectangle(pos.X, pos.Y, size.X, size.Y));

			exit = new Point(pos.X + size.X / 2, pos.Y + size.Y);
		}

		y += VarianceY();

		Point entrance;
		int pairing = 0;
		bool didStart = start;

		for (int i = 0; i < RoomCountPerTemple; ++i)
		{
			int useX = x + WorldGen.genRand.Next(-10, 11);

			if (start)
			{
				AddRoom(useX, y, RoomDatabase.PlaceRandomRoom(OpeningType.Above, useX, y, [], false), false, pairing, out entrance);
				pairing++;
				start = false;
			}
			else
			{
				AddRoom(useX, y, RoomDatabase.PlaceRandomRoom(OpeningType.Below, useX, y, [], false), true, pairing, out exit);
				pairing++;
				y += (int)(VarianceY() * 0.8f);

				AddRoom(useX, y, RoomDatabase.PlaceRandomRoom(OpeningType.Above, useX, y, [], false), false, pairing, out entrance);
				pairing++;
			}

			y += VarianceY();

			tunnels.Add((exit, entrance));
		}

		if (didStart)
		{
			string structure = "Assets/Structures/GolemDomain/Arena_0";
			Point16 pos = new(Main.maxTilesX / 2, y + 90);
			Point16 size = StructureTools.GetSize(structure);
			pos = StructureTools.PlaceByOrigin(structure, pos, new Vector2(0.5f));

			Arena = new Rectangle(pos.X, pos.Y, size.X, size.Y);

			AddRoom(pos.X, pos.Y, new PlacedRoom(new RoomData(WireColor.Yellow, OpeningType.Above, Point.Zero, new Point(67, 0), null, null, false), 
				Arena), true, pairing, out Point _);

			Arena = new Rectangle(pos.X * 16, pos.Y * 16, size.X * 16, size.Y * 16);
		}

		static int VarianceY()
		{
			return WorldGen.genRand.Next(87, 89);
		}
	}

	private static void AddRoom(int x, int y, PlacedRoom room, bool exit, int pairingNumber, out Point opening)
	{
		Rooms.Add(new PairingRoom(room, exit, pairingNumber));

		// X is already centered as that's how the placement works for our rooms here.
		if (exit)
		{
			opening = new Point(x, y + room.Area.Height);
		}
		else
		{
			opening = new Point(x, y);
		}
	}

	public static int FindTileBelow(int x, int y)
	{
		while (!WorldGen.SolidOrSlopedTile(x, y))
		{
			y++;
		}

		return y;
	}

	private void Decor(GenerationProgress progress, GameConfiguration configuration)
	{
		Dictionary<Point16, OpenFlags> grasses = [];

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (tile.HasTile && tile.TileType == TileID.Mud)
				{
					OpenFlags flags = OpenExtensions.GetOpenings(i, j, false, false);

					if (flags != OpenFlags.None)
					{
						tile.TileType = TileID.JungleGrass;

						grasses.Add(new Point16(i, j), flags);
					}
					else if (WorldGen.genRand.NextBool(180) && WorldGen.InWorld(i, j, 25) && GenVars.structures.CanPlace(new Rectangle(i, j, 1, 1), 20))
					{
						ushort type = WorldGen.genRand.NextBool() ? TileID.Mudstone : TileID.Stone;
						WorldGen.TileRunner(i, j, WorldGen.genRand.NextFloat(3, 12), WorldGen.genRand.Next(3, 12), type);
					}
				}
			}

			progress.Value = (float)i / Main.maxTilesX;
		}

		foreach (KeyValuePair<Point16, OpenFlags> grass in grasses)
		{
			Decoration.GrowOnJungleGrass(grass.Key.X, grass.Key.Y, grass.Value);
		}
	}

	private void GenTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		BossSpawned = false;
		ExitSpawned = false;

		progress.Start(1);
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");

		Main.spawnTileX = Width / 2;
		Main.spawnTileY = 100;

		Main.worldSurface = 210;
		Main.rockLayer = 260;

		FastNoiseLite noise = new(WorldGen._genRandSeed);
		noise.SetFrequency(0.01f);

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (j > 110 + Math.Abs(i - Width / 2) * 0.36f + noise.GetNoise(i, 0) * 7)
				{
					tile.HasTile = true;
					tile.TileType = TileID.Mud;
				}
			}

			progress.Value = (float)i / Main.maxTilesX;
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();

		BossSpawned = false;
		ExitSpawned = false;
	}

	public override void Update()
	{
		NPC.downedPlantBoss = true;
		Wiring.UpdateMech();
		TileEntity.UpdateStart();

		foreach (TileEntity te in TileEntity.ByID.Values)
		{
			te.Update();
		}

		TileEntity.UpdateEnd();

		if (!BossSpawned)
		{
			bool canSpawn = Main.CurrentFrameFlags.ActivePlayersCount > 0;
			HashSet<int> who = [];

			if (canSpawn)
			{
				foreach (Player player in Main.ActivePlayers)
				{
					if (!Arena.Intersects(player.Hitbox))
					{
						canSpawn = false;
						break;
					}
					else
					{
						who.Add(player.whoAmI);
					}
				}
			}

			if (canSpawn && Main.CurrentFrameFlags.ActivePlayersCount > 0 && who.Count > 0)
			{
				int plr = Main.rand.Next([.. who]);
				IEntitySource src = Entity.GetSource_NaturalSpawn();

				int npc = NPC.NewNPC(src, (int)Arena.Center().X - (Main.rand.NextBool() ? - 720 : 720), (int)Arena.Center().Y, NPCID.Golem);

				Main.spawnTileX = (int)Arena.Center().X / 16;
				Main.spawnTileY = (int)Arena.Center().Y / 16;

				if (Main.netMode == NetmodeID.Server)
				{
					NetMessage.SendData(MessageID.WorldData);
				}

				BossSpawned = true;
			}
		}

		if (BossSpawned && !NPC.AnyNPCs(NPCID.Golem) && !ExitSpawned)
		{
			ExitSpawned = true;

			HashSet<Player> players = [];

			foreach (Player plr in Main.ActivePlayers)
			{
				if (!plr.dead)
				{
					players.Add(plr);
				}
			}

			IEntitySource src = Entity.GetSource_NaturalSpawn();
			Vector2 position = Arena.Center.ToVector2() + new Vector2(0, 66);
			Projectile.NewProjectile(src, position, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);
		}
	}
}
