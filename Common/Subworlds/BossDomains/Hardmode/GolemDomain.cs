using PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode.SkeleDomain;
using PathOfTerraria.Common.Subworlds.Tools;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.World.Generation.Tools;
using PathOfTerraria.Content.Projectiles.Utility;
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
	public override int Width => 600;
	public override int Height => 1500;
	public override (int time, bool isDay) ForceTime => ((int)Main.dayLength / 2, true);

	private static readonly List<PlacedRoom> Rooms = [];

	private static bool BossSpawned = false;
	private static bool ExitSpawned = false;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Terrain", GenTerrain),
		new PassLegacy("Temples", Temples),
		new PassLegacy("Decor", Decor)];

	private void Temples(GenerationProgress progress, GameConfiguration configuration)
	{
		const int Seperation = 50;

		Rooms.Clear();
		bool start = WorldGen.genRand.NextBool();
		List<(Point, Point)> tunnels = [];

		SpawnTemple(Width / 2 - Seperation, FindTileBelow(Width / 2 - Seperation, 100), start, tunnels);
		SpawnTemple(Width / 2 + Seperation, FindTileBelow(Width / 2 + Seperation, 100), !start, tunnels);

		foreach ((Point exit, Point entrance) in tunnels)
		{
			var vExit = exit.ToVector2();
			var vEntrance = entrance.ToVector2();
			Vector2[] tunnel = Tunnel.GeneratePoints([vExit, Vector2.Lerp(vExit, vEntrance, 0.5f + WorldGen.genRand.NextFloat(-0.1f, 0.1f)) 
				- new Vector2(WorldGen.genRand.NextFloat(-22, 22), 0), vEntrance], 12, 1, 0.2f);
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
			for (int j = -8; j < 9; ++j)
			{
				var newPos = new Point16(center.X + i, center.Y + j);

				if (clearTiles.Contains(newPos) || !GenVars.structures.CanPlace(new Rectangle(newPos.X, newPos.Y, 1, 1)))
				{
					continue;
				}

				allTiles.Add(newPos);

				if (i > -5 && i < 5 && j > -5 && j < 5)
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

		for (int i = 0; i < 4; ++i)
		{
			int useX = x + WorldGen.genRand.Next(-10, 11);

			if (start)
			{
				AddRoom(useX, y, RoomDatabase.PlaceRandomRoom(OpeningType.Above, useX, y, [], false), false, out entrance);
				start = false;
			}
			else
			{
				AddRoom(useX, y, RoomDatabase.PlaceRandomRoom(OpeningType.Below, useX, y, [], false), true, out exit);
				y += (int)(VarianceY() * 0.8f);

				AddRoom(useX, y, RoomDatabase.PlaceRandomRoom(OpeningType.Above, useX, y, [], false), false, out entrance);
			}

			y += VarianceY();

			tunnels.Add((exit, entrance));
		}

		static int VarianceY()
		{
			return WorldGen.genRand.Next(70, 180);
		}
	}

	private static void AddRoom(int x, int y, PlacedRoom room, bool exit, out Point opening)
	{
		Rooms.Add(room);

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
		Wiring.UpdateMech();
		TileEntity.UpdateStart();

		foreach (TileEntity te in TileEntity.ByID.Values)
		{
			te.Update();
		}

		TileEntity.UpdateEnd();

		if (!BossSpawned && NPC.AnyNPCs(NPCID.Plantera))
		{
			BossSpawned = true;
		}

		if (BossSpawned && !NPC.AnyNPCs(NPCID.Plantera) && !ExitSpawned)
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
			Vector2 position = Main.rand.Next([.. players]).Center - new Vector2(0, 60);
			Projectile.NewProjectile(src, position, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);
		}
	}
}
