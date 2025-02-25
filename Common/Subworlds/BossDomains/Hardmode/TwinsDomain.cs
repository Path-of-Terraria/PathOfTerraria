using PathOfTerraria.Common.Subworlds.Tools;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Content.Tiles.BossDomain;
using PathOfTerraria.Content.Tiles.BossDomain.Mech;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;

internal class TwinsDomain : BossDomainSubworld
{
	public const int MazeWidth = 11;
	public const int MazeHeight = 7;

	public override int Width => 900;
	public override int Height => 1500;
	public override (int time, bool isDay) ForceTime => ((int)Main.nightLength / 2, false);

	internal static Point16 CircleCenter = Point16.Zero;

	internal static int BlockLayer => 200;
	internal static int DirtLayerEnd => 400;
	internal static int MetalLayerEnd => 910;
	internal static int MetalLayerTransEnd => MetalLayerEnd + 20;
	internal static int GateLayer => 410;

	private static bool BossSpawned = false;
	private static bool ExitSpawned = false;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Terrain", GenTerrain),
		new PassLegacy("Decor", GenDecor)];

	private void GenDecor(GenerationProgress progress, GameConfiguration configuration)
	{
		Dictionary<Point16, OpenFlags> grasses = [];
		Dictionary<Point16, OpenFlags> metals = [];

		FastNoiseLite noise = new();
		noise.SetFrequency(0.2f);

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.PopulatingWorld");

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (tile.TileType == TileID.Ebonstone)
				{
					TryPlaceEbonsand(i, j, noise);
					continue;
				}

				OpenFlags flags = OpenExtensions.GetOpenings(i, j, false, false);

				if (!tile.HasTile || flags == OpenFlags.None)
				{
					continue;
				}

				if (tile.TileType == TileID.Dirt)
				{
					tile.TileType = TileID.CorruptGrass;

					grasses.Add(new Point16(i, j), flags);
				}
				else if (tile.TileType is TileID.TinPlating or TileID.TinBrick or TileID.IronBrick or TileID.SilverBrick or TileID.PlatinumBrick)
				{
					metals.Add(new Point16(i, j), flags);
				}
			}

			progress.Value = (float)i / Main.maxTilesX;
		}

		foreach (KeyValuePair<Point16, OpenFlags> grass in grasses)
		{
			DecorateGrass(grass.Key, grass.Value);
		}

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.PopulatingMetals");

		foreach (KeyValuePair<Point16, OpenFlags> metal in metals)
		{
			DecorateMetals(metal.Key, metal.Value);
		}

		PlaceButtons();
	}

	private static void TryPlaceEbonsand(int i, int j, FastNoiseLite noise)
	{
		bool canSand = Main.tile[i, j + 1].HasTile;

		if (!canSand || !WorldGen.InWorld(i, j, 20))
		{
			return;
		}

		int height = 6 + (int)(noise.GetNoise(i, j) * 3);

		for (int y = j; y > j - height; --y)
		{
			if (!WorldGen.SolidOrSlopedTile(i, y))
			{
				Tile tile = Main.tile[i, j];
				tile.TileType = TileID.Ebonsand;
				return;
			}
		}
	}

	private void PlaceButtons()
	{
		int count = 0;
		List<Vector2> positions = [];
		int type = ModContent.TileType<MechButton>();

		while (count < 4)
		{
			int x = WorldGen.genRand.Next(10, Width - 10);
			int y = WorldGen.genRand.Next(DirtLayerEnd + 20, MetalLayerEnd);

			if (WorldGen.PlaceObject(x, y, type, true) && Main.tile[x, y].TileType == type && Main.tile[x, y].HasTile 
				&& !positions.Any(other => other.DistanceSQ(new Vector2(x, y)) < 200 * 200))
			{
				count++;

				positions.Add(new Vector2(x, y));

				while (y >= GateLayer)
				{
					Tile tile = Main.tile[x, y];
					tile.GreenWire = true;

					y--;
				}
			}
		}
	}

	private static void DecorateMetals(Point16 position, OpenFlags flags, bool fromPlatform = false)
	{
		if (!Main.tile[position].HasTile)
		{
			return;
		}

		if (WorldGen.genRand.NextBool(150) && position.X > 10 && position.X < Main.maxTilesX - 10 && !fromPlatform)
		{
			PlaceGemsparkWall(position, flags);
		}

		if (flags.HasFlag(OpenFlags.Above))
		{
			if (WorldGen.genRand.NextBool(5))
			{
				int type = ModContent.TileType<MechDecor1x1>();
				int styleRange = 8;

				if (WorldGen.genRand.NextBool(4))
				{
					type = ModContent.TileType<MechDecor2x2>();
					styleRange = 4;
				}

				WorldGen.PlaceObject(position.X, position.Y - 1, type, style: WorldGen.genRand.Next(styleRange));
			}
			else if (WorldGen.genRand.NextBool(250) && !fromPlatform)
			{
				WorldGen.PlaceObject(position.X, position.Y - 3, ModContent.TileType<MechLamp>());

				for (int i = -1; i < 11; ++i)
				{
					Tile tile = Main.tile[position.X, position.Y + i];
					tile.YellowWire = true;
				}

				Point16 timerPos = new(position.X, position.Y + 10);
				Tile timer = Main.tile[timerPos];
				timer.TileType = TileID.Timers;
				timer.TileFrameX = 0;
				timer.TileFrameY = 18;
				timer.HasTile = true;

				ModContent.GetInstance<RoomDatabase>().AddTimerInfo(new EngageTimerInfo(timerPos, WorldGen.genRand.Next(600)));
			}
		}

		if (flags.HasFlag(OpenFlags.Below))
		{
			if (WorldGen.genRand.NextBool(20))
			{
				int count = WorldGen.genRand.Next(6, 12) + 1;
				int start = Main.tile[position].TileType == ModContent.TileType<MechPlatform>() ? -1 : 1;

				for (int i = start; i < count; ++i)
				{
					Tile tile = Main.tile[position.X, position.Y + i];

					if (tile.HasTile)
					{
						if (i == 0)
						{
							continue;
						}
						else
						{
							break;
						}
					}

					tile.TileType = TileID.Chain;
					tile.HasTile = true;
				}
			}
			else if (WorldGen.genRand.NextBool(350) && !fromPlatform)
			{
				WorldGen.PlaceObject(position.X, position.Y + 1, TileID.HangingLanterns);

				for (int i = -1; i < 11; ++i)
				{
					Tile tile = Main.tile[position.X, position.Y - i];
					tile.YellowWire = true;
				}

				Point16 timerPos = new(position.X, position.Y - 10);
				Tile timer = Main.tile[timerPos];
				timer.TileType = TileID.Timers;
				timer.TileFrameX = 0;
				timer.TileFrameY = 18;
				timer.HasTile = true;

				ModContent.GetInstance<RoomDatabase>().AddTimerInfo(new EngageTimerInfo(timerPos, WorldGen.genRand.Next(600)));
			}
		}

		if (fromPlatform)
		{
			return;
		}

		if ((flags.HasFlag(OpenFlags.Left) || flags.HasFlag(OpenFlags.Right)) && position.X > 20 && position.X < Main.maxTilesX - 20 
			&& WorldGen.genRand.NextBool(10) && position.Y % 3 == 0)
		{
			bool left = true;

			if (flags.HasFlag(OpenFlags.Right) && flags.HasFlag(OpenFlags.Left))
			{
				left = WorldGen.genRand.NextBool(2);
			}
			else if (flags.HasFlag(OpenFlags.Right))
			{
				left = false;
			}

			PlacePlatforms(position.X, position.Y, left);
		}
	}

	private static void PlacePlatforms(short x, short y, bool left)
	{
		int len = WorldGen.genRand.Next(5, 11);

		for (int i = 1; i < len; ++i)
		{
			int realX = x + i * (left ? -1 : 1);
			Tile tile = Main.tile[realX, y];
			tile.TileType = (ushort)ModContent.TileType<MechPlatform>();
			tile.HasTile = true;

			WorldGen.TileFrame(realX, y);
			DecorateMetals(new Point16(realX, y), OpenFlags.Above | OpenFlags.Below, true);
		}
	}

	private static void PlaceGemsparkWall(Point16 position, OpenFlags flags)
	{
		if (flags == OpenFlags.None)
		{
			return;
		}

		HashSet<OpenFlags> directions = [];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void AddIfTrue(OpenFlags flag)
		{
			if (flags.HasFlag(flag))
			{
				directions.Add(flag);
			}
		}

		AddIfTrue(OpenFlags.Above);
		AddIfTrue(OpenFlags.Below);
		AddIfTrue(OpenFlags.Left);
		AddIfTrue(OpenFlags.Right);

		if (directions.Count == 0)
		{
			return;
		}

		OpenFlags flag = WorldGen.genRand.Next([.. directions]);

		Point direction = flag switch
		{
			OpenFlags.Above => new Point(0, -1),
			OpenFlags.Below => new Point(0, 1),
			OpenFlags.Right => new Point(1, 0),
			_ => new Point(-1, 0)
		};

		Point16 place = new(position.X + direction.X, position.Y + direction.Y);
		ushort wall = WallID.DiamondGemspark;

		if (place.Y < MetalLayerEnd + DirtLayerEnd / 2)
		{
			wall = WallID.RubyGemspark;
		}

		while (!WorldGen.SolidOrSlopedTile(place.X, place.Y))
		{
			Tile tile = Main.tile[place.X, place.Y];
			tile.WallType = wall;
			tile.BlueWire = true;

			place = new Point16(place.X + direction.X, place.Y + direction.Y);
		}
	}

	private static void DecorateGrass(Point16 position, OpenFlags flags)
	{
		if (!Main.tile[position].HasTile || Main.tile[position].TileType != TileID.CorruptGrass)
		{
			return;
		}

		if (flags.HasFlag(OpenFlags.Above))
		{
			if (!WorldGen.genRand.NextBool(3))
			{
				int id = WorldGen.genRand.Next(15);
				int type = TileID.CorruptPlants;

				if (id == 14)
				{
					type = TileID.SmallPiles;
				}

				int styleRange = id == 0 ? 8 : 23;

				WorldGen.PlaceTile(position.X, position.Y - 1, type, style: WorldGen.genRand.Next(styleRange));
			}
			else if (WorldGen.genRand.NextBool(10) && position.Y < DirtLayerEnd)
			{
				WorldGen.PlaceTile(position.X, position.Y - 1, TileID.Saplings);

				if (!WorldGen.GrowTree(position.X, position.Y - 1))
				{
					WorldGen.KillTile(position.X, position.Y - 1);
				}
			}
		}

		if (flags.HasFlag(OpenFlags.Below) && WorldGen.genRand.NextBool(3, 5))
		{
			int count = WorldGen.genRand.Next(4, 9) + 1;

			for (int i = 1; i < count; ++i)
			{
				Tile tile = Main.tile[position.X, position.Y + i];

				if (tile.HasTile)
				{
					break;
				}

				tile.TileType = TileID.CorruptVines;
				tile.HasTile = true;
			}
		}
	}

	private void GenTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		FastNoiseLite malaiseNoise = new(WorldGen._genRandSeed);
		malaiseNoise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
		malaiseNoise.SetFrequency(0.04f);
		malaiseNoise.SetCellularJitter(1.500f);
		malaiseNoise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2Reduced);
		malaiseNoise.SetDomainWarpAmp(182.500f);

		FastNoiseLite dirtNoise = new(WorldGen._genRandSeed);
		dirtNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
		dirtNoise.SetFrequency(0.022f);
		dirtNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
		dirtNoise.SetFractalOctaves(4);
		dirtNoise.SetFractalLacunarity(4.790f);
		dirtNoise.SetFractalGain(0.1f);
		dirtNoise.SetFractalWeightedStrength(3.140f);
		
		CreateOreNoises(out FastNoiseLite oreNoise, out FastNoiseLite oreTypeNoise);

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 20; ++j)
			{
				Tile tile = Main.tile[i, j];
				int offset = (int)(dirtNoise.GetNoise(i, 0) * 6);
				j -= offset;

				if (j > BlockLayer)
				{
					tile.HasTile = true;

					if (j >= MetalLayerTransEnd)
					{
						ForceCorruptionTile(i, j, malaiseNoise, dirtNoise);
					}
					else if (j > MetalLayerEnd)
					{
						int dif = MetalLayerTransEnd - (j + offset);
						float chance = MathF.Max(dif / 20f, 0);

						if (WorldGen.genRand.NextFloat() > chance)
						{
							ForceCorruptionTile(i, j, malaiseNoise, dirtNoise);
						}
						else
						{
							PlaceBrick(i, j, oreNoise, oreTypeNoise);
						}
					}
					else if (j > DirtLayerEnd)
					{
						PlaceBrick(i, j, oreNoise, oreTypeNoise);
					}
					else
					{
						bool isStone = dirtNoise.GetNoise(i, j) > 0.2f && j > BlockLayer + 15;
						tile.TileType = isStone ? TileID.Ebonstone : TileID.Dirt;

						if (j > BlockLayer + 2)
						{
							tile.WallType = isStone ? WallID.EbonstoneUnsafe : WallID.DirtUnsafe;
						}
					}
				}

				j += offset;
			}

			progress.Value = (float)i / Main.maxTilesX;
		}

		Main.spawnTileX = Width / 2;
		Main.spawnTileY = Height - 100;
		Main.worldSurface = BlockLayer + 10;
		Main.rockLayer = DirtLayerEnd;
		
		int spawnId = Main.rand.Next(3);

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Tunnels");
		HashSet<int> additionalEntrances = [];
		Vector2 mazeStart = DigCorruptionTunnels(StructureTools.GetSize("Assets/Structures/TwinsDomain/Spawn_" + spawnId), additionalEntrances);

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Maze");
		Point16 mazeEnd = DigMazeArea(mazeStart, progress, additionalEntrances);

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			Tile tile = Main.tile[i, GateLayer];
			tile.GreenWire = true;
			
			if (!tile.HasTile)
			{
				tile.TileType = (ushort)ModContent.TileType<MechGate>();
				tile.HasTile = true;
			}
		}

		for (int i = 0; i < 4; ++i)
		{
			DigRoughTunnel(mazeEnd.ToVector2(), new(Width / 2 + WorldGen.genRand.Next(-250, 250), BlockLayer + 100),
				new Vector2(Width / 2 + WorldGen.genRand.Next(-100, 100), BlockLayer - 40), dirtNoise, null, 0.5f);
		}

		StructureTools.PlaceByOrigin("Assets/Structures/TwinsDomain/Spawn_" + spawnId, new(Main.spawnTileX, Main.spawnTileY), new Vector2(0.5f));
	}

	private static void CreateOreNoises(out FastNoiseLite oreNoise, out FastNoiseLite oreTypeNoise)
	{
		oreNoise = new(WorldGen._genRandSeed);
		oreNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
		oreNoise.SetFrequency(0.05f);
		oreNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
		oreNoise.SetFractalOctaves(4);
		oreNoise.SetFractalLacunarity(4.790f);
		oreNoise.SetFractalGain(0.1f);
		oreNoise.SetFractalWeightedStrength(3.140f);

		oreTypeNoise = new(WorldGen._genRandSeed);
		oreTypeNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
		oreTypeNoise.SetFrequency(0.05f);
		oreTypeNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
		oreTypeNoise.SetFractalOctaves(4);
		oreTypeNoise.SetFractalLacunarity(4.790f);
		oreTypeNoise.SetFractalGain(0.1f);
		oreTypeNoise.SetFractalWeightedStrength(3.140f);
	}

	private static void PlaceBrick(int i, int j, FastNoiseLite oreNoise, FastNoiseLite oreTypeNoise)
	{
		Tile tile = Main.tile[i, j];
		tile.TileType = TileID.TinPlating;
		tile.WallType = (ushort)ModContent.WallType<TinPlatingUnsafe>();
		tile.TileColor = PaintID.GrayPaint;
		tile.WallColor = PaintID.GrayPaint;

		if (oreNoise.GetNoise(i, j) > 0.05f)
		{
			int typeNoise = (int)(oreTypeNoise.GetNoise(i, j) * 100 / 25f);

			(ushort type, ushort wall) = typeNoise switch
			{
				0 => (TileID.TinBrick, (ushort)ModContent.WallType<TinBrickUnsafe>()),
				1 => (TileID.IronBrick, (ushort)ModContent.WallType<IronBrickUnsafe>()),
				2 => (TileID.SilverBrick, (ushort)ModContent.WallType<SilverBrickUnsafe>()),
				_ => (TileID.PlatinumBrick, (ushort)ModContent.WallType<PlatinumBrickUnsafe>()),
			};

			tile.TileType = type;
			tile.WallType = wall;
			tile.TileColor = PaintID.None;

			if (wall != ModContent.WallType<PlatinumBrickUnsafe>() && wall != ModContent.WallType<TinBrickUnsafe>())
			{
				tile.WallColor = PaintID.None;
			}
		}
	}

	private static void ForceCorruptionTile(int i, int j, FastNoiseLite malaiseNoise, FastNoiseLite dirtNoise)
	{
		Tile tile = Main.tile[i, j];
		tile.TileType = TileID.Ebonstone;
		tile.WallType = WallID.EbonstoneUnsafe;

		float x = i;
		float y = j;
		malaiseNoise.DomainWarp(ref x, ref y);

		if (dirtNoise.GetNoise(i, j) > 0.4f)
		{
			tile.TileType = TileID.Dirt;
			tile.WallType = WallID.CorruptGrassUnsafe;
		}

		if (malaiseNoise.GetNoise(x, y) > -0.15f)
		{
			tile.TileType = (ushort)ModContent.TileType<WeakMalaise>();
		}
	}

	private Point16 DigMazeArea(Vector2 mazeStart, GenerationProgress progress, HashSet<int> additionalEntrances)
	{
		float maxY = mazeStart.Y;
		Range xRange = (Width / 2 - 5)..(Width / 2 + 5);
		Dictionary<Point16, bool> SideByPosition = [];

		HashSet<Point16> mazePoints = [];
		var positionBySlots = new Point16[MazeWidth, MazeHeight];
		var flagsBySlots = new OpenFlags[MazeWidth, MazeHeight];

		for (int i = 0; i < MazeWidth; ++i)
		{
			for (int j = 0; j < MazeHeight; ++j)
			{
				GetPositionOfMazeFromCell(i, j, out int x, out int y);

				mazePoints.Add(new(x, y));
				positionBySlots[i, j] = new Point16(x, y + WorldGen.genRand.Next(-3, 4));

				int connections = WorldGen.genRand.Next(1, 4);

				for (int k = 0; k < connections; ++k)
				{
					OpenFlags flag = WorldGen.genRand.Next(4) switch
					{
						0 => OpenFlags.Above,
						1 => OpenFlags.Below,
						2 => OpenFlags.Left,
						_ => OpenFlags.Right,
					};

					if (j == 0 && flag == OpenFlags.Above)
					{
						k--;
						continue;
					}

					if (j == MazeHeight - 1 && flag == OpenFlags.Below)
					{
						k--;
						continue;
					}

					if (i == 0 && flag == OpenFlags.Left)
					{
						k--;
						continue;
					}

					if (i == MazeWidth - 1 && flag == OpenFlags.Right)
					{
						k--;
						continue;
					}

					flagsBySlots[i, j] |= flag;
				}
			}

			progress.Set(i / (MazeWidth - 1f));
		}

		int exitPoint = WorldGen.genRand.Next(2, MazeWidth - 2);
		flagsBySlots[exitPoint, 0] |= OpenFlags.Above;
		flagsBySlots[MazeWidth / 2, MazeHeight - 1] |= OpenFlags.Below;

		foreach (int xPosition in additionalEntrances)
		{
			flagsBySlots[xPosition, MazeHeight - 1] |= OpenFlags.Below;
		}

		ForcePath(new Point16(MazeWidth / 2, MazeHeight - 1), new Point16(exitPoint, 0), MazeWidth, MazeHeight, flagsBySlots);

		for (int i = 0; i < MazeWidth; ++i)
		{
			for (int j = 0; j < MazeHeight; ++j)
			{
				DigMazeConnector(i, j, positionBySlots, flagsBySlots, exitPoint);
			}
		}

		Point16 endStart = positionBySlots[exitPoint, 0];
		return new(endStart.X, endStart.Y - 100);
	}

	private void GetPositionOfMazeFromCell(int i, int j, out int x, out int y)
	{
		x = (int)MathHelper.Lerp(80, Width - 80, i / (float)(MazeWidth - 1));
		y = (int)MathHelper.Lerp(DirtLayerEnd + 40, MetalLayerEnd - 40, j / (float)(MazeHeight - 1));
	}

	private static void ForcePath(Point16 start, Point16 end, int width, int height, OpenFlags[,] flagsBySlots)
	{
		Dictionary<Point, OpenFlags> takenIndexes = [];
		var current = start.ToPoint();

		while (current.X != end.X || current.Y == end.Y)
		{
			if (AllSurroundsInvalid(current.X, current.Y))
			{
				takenIndexes.Clear();
				current = start.ToPoint();
				continue;
			}

			OpenFlags flag = WorldGen.genRand.Next(4) switch
			{
				0 => OpenFlags.Above,
				1 => OpenFlags.Below,
				2 => OpenFlags.Left,
				_ => OpenFlags.Right,
			};

			Point offset = flag switch
			{
				OpenFlags.Left => new Point(-1, 0),
				OpenFlags.Right => new Point(1, 0),
				OpenFlags.Below => new Point(0, 1),
				_ => new Point(0, -1)
			};

			var newPos = new Point(current.X + offset.X, current.Y + offset.Y);

			if (current == new Point(end.X, end.Y + 1) || current == new Point(end.X + 1, end.Y) || current == new Point(end.X - 1, end.Y))
			{
				break;
			}

			if (newPos == end.ToPoint())
			{
				break;
			}

			if (OutsideOfBounds(newPos.X, newPos.Y) || takenIndexes.ContainsKey(newPos))
			{
				continue;
			}

			takenIndexes.Add(current, flag);
			current = newPos;
		}

		foreach (KeyValuePair<Point, OpenFlags> pair in takenIndexes)
		{
			flagsBySlots[pair.Key.X, pair.Key.Y] |= pair.Value;
		}

		return;

		bool OutsideOfBounds(int x, int y)
		{
			return x < 0 || y < 0 || x >= width - 1 || y >= height - 1;
		}

		bool AllSurroundsInvalid(int x, int y)
		{
			if (!OutsideOfBounds(x - 1, y) && !takenIndexes.ContainsKey(new Point(x - 1, y)))
			{
				return false;
			}

			if (!OutsideOfBounds(x + 1, y) && !takenIndexes.ContainsKey(new Point(x + 1, y)))
			{
				return false;
			}

			if (!OutsideOfBounds(x, y - 1) && !takenIndexes.ContainsKey(new Point(x, y - 1)))
			{
				return false;
			}

			if (!OutsideOfBounds(x, y + 1) && !takenIndexes.ContainsKey(new Point(x, y + 1)))
			{
				return false;
			}

			return true;
		}
	}

	private static void DigMazeConnector(int i, int j, Point16[,] positionBySlots, OpenFlags[,] flagsBySlots, int exitPoint)
	{
		Point16 position = positionBySlots[i, j];
		OpenFlags connections = flagsBySlots[i, j];
		int width = positionBySlots.GetLength(0);
		
		if (connections.HasFlag(OpenFlags.Above) && (j > 0 || i == exitPoint))
		{
			int aboveY = j == 0 ? position.Y - 100 : positionBySlots[i, j - 1].Y - 6;

			for (int y = position.Y; y >= aboveY; y--)
			{
				for (int x = -6; x < 7; ++x)
				{
					WorldGen.KillTile(position.X + x, y);
				}
			}
		}

		if (connections.HasFlag(OpenFlags.Below) && j < positionBySlots.GetLength(1))
		{
			int belowY = j == positionBySlots.GetLength(1) - 1 ? position.Y + 60 : positionBySlots[i, j + 1].Y + 6;

			for (int y = position.Y; y <= belowY; y++)
			{
				for (int x = -6; x < 7; ++x)
				{
					WorldGen.KillTile(position.X + x, y);
				}
			}
		}

		if (connections.HasFlag(OpenFlags.Left) && i > 0)
		{
			for (int x = position.X + 6; x >= positionBySlots[i - 1, j].X - 6; x--)
			{
				for (int y = -6; y < 7; ++y)
				{
					WorldGen.KillTile(x, position.Y + y);
				}
			}
		}

		if (connections.HasFlag(OpenFlags.Right) && i < width - 1)
		{
			for (int x = position.X - 6; x <= positionBySlots[i + 1, j].X + 6; x++)
			{
				for (int y = -6; y < 7; ++y)
				{
					WorldGen.KillTile(x, position.Y + y);
				}
			}
		}
	}

	private Vector2 DigCorruptionTunnels(Point16 spawnSize, HashSet<int> additionalEntrances)
	{
		Vector2 start = new(Main.spawnTileX, Main.spawnTileY - spawnSize.Y / 2);
		FastNoiseLite noise = new(WorldGen._genRandSeed);
		int tunnelCount = Main.rand.Next(4, 6);
		List<Vector2> ends = [];

		DigRoughTunnel(start, start - new Vector2(WorldGen.genRand.Next(-5, 5), 40), start - new Vector2(0, 80), noise);
		start.Y -= 80;

		for (int i = 0; i < tunnelCount; ++i) 
		{
			float midPointX = MathHelper.Lerp(300, Width - 300, i / (tunnelCount - 1f)) + WorldGen.genRand.Next(-40, 40);
			float endPointX = MathHelper.Lerp(150, Width - 150, i / (tunnelCount - 1f)) + WorldGen.genRand.Next(-80, 80);
			Vector2 end = new(endPointX, start.Y - 180);
			DigRoughTunnel(start, new Vector2(midPointX, start.Y - 100), end, noise, ends);

			ends.Add(end);
		}

		while (additionalEntrances.Count < 3)
		{
			int random = WorldGen.genRand.Next(MazeWidth);

			while (additionalEntrances.Contains(random) || random == MazeWidth / 2)
			{
				random = WorldGen.genRand.Next(MazeWidth);
			}

			additionalEntrances.Add(random);
		}

		for (int i = 0; i < ends.Count; ++i) 
		{
			Vector2 newStart = ends[i];
			Vector2 end = new(Width / 2, newStart.Y - 180);

			float midPointX = MathHelper.Lerp(300, Width - 300, i / (tunnelCount - 1f)) + WorldGen.genRand.Next(-40, 40);
			Vector2 mid = new(midPointX, newStart.Y - 100);

			if (i >= tunnelCount)
			{
				mid = Vector2.Lerp(newStart, end, WorldGen.genRand.NextFloat(0.45f, 0.55f));
				end = Main.rand.Next(ends);
			}

			if (i > ends.Count - 1 - additionalEntrances.Count)
			{
				int index = i - (ends.Count - 1 - additionalEntrances.Count) - 1;
				GetPositionOfMazeFromCell(additionalEntrances.ElementAt(index), MazeHeight - 1, out int x, out int y);
				end = new Vector2(x, y + 60);
			}

			DigRoughTunnel(newStart, mid, end, noise);
		}

		var endBase = new Vector2(Width / 2, start.Y - 360);

		for (int i = 0; i < 80; ++i)
		{
			for (int j = -6; j < 7; ++j)
			{
				Tile tile = Main.tile[(int)endBase.X + j, (int)endBase.Y - i];
				tile.HasTile = false;
			}
		}

		return endBase - new Vector2(0, 40);
	}

	private static void DigRoughTunnel(Vector2 start, Vector2 mid, Vector2 end, FastNoiseLite noise, List<Vector2> ends = null, float variationMult = 1f)
	{
		Vector2[] positions = Tunnel.GeneratePoints([start, mid, end], 8, 6, variationMult);

		foreach (Vector2 pos in positions)
		{
			TunnelDig(noise, pos);
		}

		ends?.Add(WorldGen.genRand.Next(positions));
	}

	private static void TunnelDig(FastNoiseLite noise, Vector2 pos)
	{
		float mul = 0.8f + MathF.Abs(noise.GetNoise(pos.X, pos.Y));
		Digging.CircleOpening(pos + WorldGen.genRand.NextVector2Circular(4, 4), 8 * mul);
		Digging.CircleOpening(pos, WorldGen.genRand.Next(3, 7) * mul);

		if (WorldGen.genRand.NextBool(3, 5))
		{
			WorldGen.digTunnel(pos.X, pos.Y, 0, 0, 5, (int)(WorldGen.genRand.NextFloat(5, 9) * mul));
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
		
		if (!BossSpawned)
		{
			bool canSpawn = Main.CurrentFrameFlags.ActivePlayersCount > 0;
			HashSet<int> who = [];

			foreach (Player player in Main.ActivePlayers)
			{
				if (player.position.Y > BlockLayer * 16 + 200)
				{
					canSpawn = false;
					break;
				}
				else
				{
					who.Add(player.whoAmI);
				}
			}

			if (canSpawn)
			{
				int plr = Main.rand.Next([.. who]);
				NPC.SpawnOnPlayer(plr, NPCID.Spazmatism);
				NPC.SpawnOnPlayer(plr, NPCID.Retinazer);

				BossSpawned = true;
			}
		}
		else
		{
			if (!NPC.AnyNPCs(NPCID.Spazmatism) && !NPC.AnyNPCs(NPCID.Retinazer) && !ExitSpawned)
			{
				ExitSpawned = true;

				IEntitySource src = Entity.GetSource_NaturalSpawn();
				Projectile.NewProjectile(src, new Vector2(Width / 2, BlockLayer - 16).ToWorldCoordinates(), Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);
			}
		}
	}
}
