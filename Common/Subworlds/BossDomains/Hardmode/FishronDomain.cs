using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.World.Passes;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Content.Tiles.BossDomain.Mushroom;
using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Systems.BossTrackingSystems;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;

internal class FishronDomain : BossDomainSubworld, IOverrideBiome
{
	public const int FloorY = 180;
	public const int WaterLine = FloorY + 20;
	public const int MushroomCount = 7;

	public override int Width => 1300;
	public override int Height => 600;
	public override (int time, bool isDay) ForceTime => (4600, true);
	public override int[] WhitelistedMiningTiles => [ModContent.TileType<Burstshroom2x2>(), TileID.MushroomBlock];

	internal static int MushroomsBroken = 0;

	private static bool BossSpawned = false;
	private static bool ExitSpawned = false;
	private static int BossSpawnTimer = 0;
	private static Point16 NewSpawn = Point16.Zero;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Terrain", Terrain),
		new PassLegacy("Decor", DecorateWorld),
		new PassLegacy("SettleLiquids", SettleLiquidsStep.Generation),
		new PassLegacy("Pads", AddPads)];

	private static void Terrain(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");
		Main.worldSurface = Main.maxTilesY - 42; // Hides the underground layer just out of bounds
		Main.rockLayer = Main.maxTilesY; // Hides the cavern layer way out of bounds

		FastNoiseLite noise = FlatNoise();
		float noiseAmp = 20;
		int tileId = TileID.Mud;
		int wallId = WallID.MushroomUnsafe;

		for (int x = 0; x < Main.maxTilesX; x++)
		{
			for (int y = 0; y < Main.maxTilesY; y++)
			{
				progress.Set((y + x * Main.maxTilesY) / (float)(Main.maxTilesX * Main.maxTilesY)); // Controls the progress bar, should only be set between 0f and 1f
				Tile tile = Main.tile[x, y];

				float warpedX = x;
				float warpedY = y;
				noise.DomainWarp(ref warpedX, ref warpedY);

				int floorY = (int)(FloorY + (noise is null ? 0 : noise.GetNoise(warpedX, 0) * noiseAmp));

				if (y <= floorY)
				{
					continue; // Stop tiles from being placed above the floor
				}

				tile.HasTile = true;
				tile.TileType = (ushort)tileId;

				if (y > floorY + 1)
				{
					float value = noise.GetNoise(warpedX, warpedY);

					if (value < 0f)
					{
						tile.WallType = noise.GetNoise(warpedX * 1.4f + 3000, warpedY * 1.4f) < 0f ? WallID.MudUnsafe : (ushort)wallId;
					}
					else
					{
						tile.WallType = WallID.None;
					}
				}
			}
		}
	}

	private void AddPads(GenerationProgress progress, GameConfiguration configuration)
	{
		int totalRepeats = 0;
		List<int> xs = [];

		for (int i = 0; i < 9; ++i)
		{
			int x;

			do
			{
				x = WorldGen.genRand.Next(200, Width - 200);
				totalRepeats++;

				if (totalRepeats > 30000)
				{
					return;
				}
			} while (Collision.SolidCollision(new Vector2((x - 10) * 16, WaterLine * 16), 20 * 16, 16) || xs.Any(v => Math.Abs(v - x) < 36));

			xs.Add(x);
			PlaceMushroomLily(x, WaterLine + 1);

			if (Math.Abs(NewSpawn.X - Width / 2) > Math.Abs(x - Width / 2))
			{
				NewSpawn = new Point16(x, WaterLine - 3);
			}
		}
	}

	internal static void PlaceMushroomLily(int x, int y)
	{
		int width = WorldGen.genRand.Next(9, 21);

		for (int i = x - width + 1; i < x + width; ++i)
		{
			Tile tile = Main.tile[i, y];
			tile.HasTile = true;
			tile.TileType = (ushort)ModContent.TileType<Mushpad>();

			if (i == x)
			{
				for (int j = 1; j < width / 3; ++j)
				{
					Tile stem = Main.tile[i, y + j];
					stem.HasTile = true;
					stem.TileType = (ushort)ModContent.TileType<Mushpad>();
				}
			}
		}

		for (int i = x - width; i < x + width; ++i)
		{
			WorldGen.TileFrame(i, y);

			if (i == x)
			{
				for (int j = 1; j < width / 3; ++j)
				{
					WorldGen.TileFrame(i, y + j);
				}
			}
			else
			{
				SpawnMushroomVine(i, y);
			}
		}

		if (!WorldGen.genRand.NextBool(3))
		{
			WorldGen.PlaceObject(x, y - 1, ModContent.TileType<MushpadFlower>(), true, WorldGen.genRand.Next(2));
		}
	}

	private void DecorateWorld(GenerationProgress progress, GameConfiguration configuration)
	{
		MushroomsBroken = 0;
		BossSpawnTimer = 0;
		NewSpawn = Point16.Zero;
		BossSpawned = false;
		ExitSpawned = false;

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Hole"); // Sets the text displayed for this pass
		GeneratePit();

		Main.spawnTileX = WorldGen.genRand.NextBool() ? 120 : Main.maxTilesX - 120;
		Main.spawnTileY = 140;

		while (!WorldGen.SolidOrSlopedTile(Main.spawnTileX, Main.spawnTileY))
		{
			Main.spawnTileY++;
		}

		Main.spawnTileY -= 3;

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.PopulatingWorld");

		Dictionary<Point16, OpenFlags> grasses = [];

		int seed = Main.rand.Next();
		FastNoiseLite wallNoise = new(seed);
		wallNoise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
		wallNoise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.EuclideanSq);
		wallNoise.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Div);
		wallNoise.SetCellularJitter(1.120f);
		wallNoise.SetFrequency(0.025f);
		wallNoise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2);
		wallNoise.SetDomainWarpAmp(138.5f);

		FastNoiseLite stoneNoise = new(seed);
		stoneNoise.SetFrequency(0.027f);
		stoneNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
		stoneNoise.SetFractalWeightedStrength(4.38f);

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; ++j)
			{
				Tile tile = Main.tile[i, j];

				Tile.SmoothSlope(i, j, false);

				if (tile.HasTile && stoneNoise.GetNoise(i, j) < -0.7f)
				{
					tile.TileType = TileID.Stone;
				}

				if (tile.HasTile && tile.TileType == TileID.Mud)
				{
					OpenFlags flags = OpenExtensions.GetOpenings(i, j, false, false);

					if (flags != OpenFlags.None)
					{
						tile.TileType = TileID.MushroomGrass;

						if (tile.Slope == SlopeType.Solid)
						{
							grasses.Add(new Point16(i, j), flags);
						}
					}
					else if (WorldGen.genRand.NextBool(80))
					{
						tile.TileType = TileID.MushroomGrass;
					}
				}

				float wallX = i;
				float wallY = j;

				wallNoise.DomainWarp(ref wallX, ref wallY);

				if (wallNoise.GetNoise(wallX, wallY) > -0.06f && tile.WallType != WallID.None && !tile.HasTile)
				{
					WorldGen.PlaceTile(i, j, ModContent.TileType<MushroomGrowths>(), true);
				}
			}

			progress.Value = (float)i / Main.maxTilesX;
		}

		int count = 0;

		foreach (KeyValuePair<Point16, OpenFlags> grass in grasses)
		{
			GrowOnGrass(grass.Key.X, grass.Key.Y, grass.Value);
			progress.Value = (float)count / grasses.Count;
			count++;
		}

		int burstshroomCount = 0;
		HashSet<Point16> burstshrooms = [];

		while (burstshroomCount < MushroomCount)
		{
			Point16 pos;

			do
			{
				pos = new Point16(WorldGen.genRand.Next(200, Width - 200), WorldGen.genRand.Next(WaterLine, Height - 120));
			} while (!grasses.ContainsKey(pos) || Main.tile[pos.X, pos.Y - 1].HasTile || burstshrooms.Any(x => x.ToVector2().DistanceSQ(pos.ToVector2()) < 30 * 30));

			WorldGen.PlaceObject(pos.X, pos.Y - 1, ModContent.TileType<Burstshroom2x2>(), true, style: WorldGen.genRand.Next(4));

			if (Main.tile[pos.X, pos.Y - 1].TileType == ModContent.TileType<Burstshroom2x2>())
			{
				burstshroomCount++;
				burstshrooms.Add(pos);
			}
		}

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (j > WaterLine)
				{
					tile.LiquidType = LiquidID.Water;
					tile.LiquidAmount = 255;
				}
			}

			progress.Value = (float)i / Main.maxTilesX;
		}

		Main.worldSurface = FloorY + 60;
		Main.rockLayer = FloorY + 65;
	}

	private void GeneratePit()
	{
		int x = Width / 2;
		int y = 100;

		while (!WorldGen.SolidOrSlopedTile(x, y))
		{
			y++;
		}

		y += 79;

		ShapeData data = new();

		WorldUtils.Gen(new Point(x, y), new Shapes.Circle(300, 90),
			Actions.Chain(new Modifiers.Blotches(4, 4, 0.7f), new Actions.ClearTile().Output(data)));

		int count = WorldGen.genRand.Next(9, 12);
		List<Point> points = [];

		for (int i = 0; i < count + 1; ++i)
		{
			Point origin;

			do
			{
				origin = WorldGen.genRand.NextVector2CircularEdge(260, WorldGen.genRand.Next(120, 140)).ToPoint();
			} while (origin.Y < 0 || points.Any(x => x.ToVector2().Distance(origin.ToVector2()) < 50));

			GenShape shape = new Shapes.Circle(WorldGen.genRand.Next(40, 60), WorldGen.genRand.Next(30, 50));
			var blotches = new Modifiers.Blotches(30, 8, 0.4f);
			WorldUtils.Gen(new Point(origin.X + x, origin.Y + y), shape, Actions.Chain(blotches, new Actions.ClearTile().Output(data)));
		}

		WorldUtils.Gen(new Point(x, y - 40), new Shapes.Rectangle(8, 40),
			Actions.Chain(new Modifiers.Blotches(5, 3, 0.6f), new Actions.ClearTile().Output(data)));
	}

	private static void GrowOnGrass(short x, short y, OpenFlags value)
	{
		if (value.HasFlag(OpenFlags.Above) && !Main.tile[x, y - 1].HasTile)
		{
			if (WorldGen.genRand.NextBool(30) && y > WaterLine)
			{
				WorldGen.PlaceObject(x, y - 1, ModContent.TileType<Bubbleshroom>(), true);

				if (Main.tile[x, y - 1].TileType == ModContent.TileType<Bubbleshroom>())
				{
					ModContent.GetInstance<Bubbleshroom.BubblerTE>().Place(x - 2, y - 2);
				}
			}
			else if (!WorldGen.genRand.NextBool(2))
			{
				WorldGen.PlaceTile(x, y - 1, TileID.MushroomPlants, true);
			}
			else if (WorldGen.genRand.NextBool(5))
			{
				WorldGen.GrowTree(x, y);
			}
		}

		if (value.HasFlag(OpenFlags.Below))
		{
			SpawnMushroomVine(x, y);
		}
	}

	private static void SpawnMushroomVine(int x, int y)
	{
		int vineType = WorldGen.genRand.NextBool(3) ? ModContent.TileType<Flowervine>() : TileID.MushroomVines;

		if (!WorldGen.genRand.NextBool(3))
		{
			int length = WorldGen.genRand.Next(5, 16);

			for (int k = 1; k < length; ++k)
			{
				if (Main.tile[x, y + k].HasTile)
				{
					break;
				}

				WorldGen.PlaceTile(x, y + k, vineType, true);
			}
		}
	}

	private static FastNoiseLite FlatNoise()
	{
		FastNoiseLite noise = new(Main.rand.Next());
		noise.SetFrequency(0.006f);
		noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
		noise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Hybrid);
		noise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2);
		noise.SetDomainWarpAmp(-900);
		return noise;
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

		if (!BossSpawned && MushroomsBroken >= 7)
		{
			BossSpawnTimer++;

			if (BossSpawnTimer == 1)
			{
				string key = $"Mods.{PoTMod.ModName}.Misc.FishronSpawn";

				if (!Main.dedServ)
				{
					Main.NewText(Language.GetTextValue(key), Colors.RarityDarkPurple);
				}
				else
				{
					ChatHelper.BroadcastChatMessage(NetworkText.FromKey(key), Colors.RarityDarkPurple);
				}
			}

			if (BossSpawnTimer > 1200)
			{
				string key = "Announcement.HasAwoken";

				if (!Main.dedServ)
				{
					Main.NewText(Language.GetTextValue(key, Lang.GetNPCName(NPCID.DukeFishron)), Colors.RarityDarkPurple);
				}
				else
				{
					ChatHelper.BroadcastChatMessage(NetworkText.FromKey(key, NetworkText.FromKey(Lang.GetNPCName(NPCID.DukeFishron).Key)), Colors.RarityDarkPurple);
				}

				List<Player> players = [];

				foreach (Player player in Main.ActivePlayers)
				{
					players.Add(player);
				}

				Vector2 pos = Main.rand.Next([.. players]).Center - new Vector2(0, 1200);
				NPC.NewNPC(Entity.GetSource_NaturalSpawn(), (int)pos.X, (int)pos.Y, NPCID.DukeFishron);

				BossSpawned = true;

				Main.spawnTileX = NewSpawn.X;
				Main.spawnTileY = NewSpawn.Y;

				if (Main.netMode == NetmodeID.Server)
				{
					NetMessage.SendData(MessageID.WorldData);
				}
			}
		}
		else if (BossSpawned && !NPC.AnyNPCs(NPCID.DukeFishron) && !ExitSpawned)
		{
			BossTracker.AddDowned(NPCID.DukeFishron, false, true);

			ExitSpawned = true;

			IEntitySource src = Entity.GetSource_NaturalSpawn();
			Projectile.NewProjectile(src, NewSpawn.ToWorldCoordinates(0, -60), Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);
		}
	}

	public void OverrideBiome()
	{
		Main.LocalPlayer.ZoneGlowshroom = true;
		Main.SmoothedMushroomLightInfluence = MushroomsBroken / (float)MushroomCount;
		Main.newMusic = MusicID.Mushrooms;
		Main.curMusic = MusicID.Mushrooms;
		Main.bgStyle = SurfaceBackgroundID.Mushroom;
	}
}
