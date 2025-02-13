using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Content.Tiles.BossDomain;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;

internal class QueenSlimeDomain : BossDomainSubworld
{
	public override int Width => 1200;
	public override int Height => 600;
	public override (int time, bool isDay) ForceTime => (3500, true);
	public override int[] WhitelistedMiningTiles => [ModContent.TileType<BrittleCrystal>()];

	private static bool LeftSpawn = false;
	private static Point16 ArenaPos = Point16.Zero;
	private static bool BossSpawned = false;
	private static bool PortalSpawned = false;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Base Terrain", SpawnBaseTerrain),
		new PassLegacy("Structures", SpawnStructures),
		new PassLegacy("Decor", SpawnDecor)];

	public override void OnEnter()
	{
		base.OnEnter();

		BossSpawned = false;
		PortalSpawned = false;
	}

	private void SpawnStructures(GenerationProgress progress, GameConfiguration configuration)
	{
		int x = Main.maxTilesX / 2 + (LeftSpawn ? 70 : -70);
		int y = 160;

		ArenaPos = new Point16(x, y);
		StructureTools.PlaceByOrigin("Assets/Structures/QueenSlimeDomain/Arena_0", ArenaPos, new Vector2(0.5f, 0.3f));
		ArenaPos = new Point16(ArenaPos.X - 4, ArenaPos.Y + 30);
	}

	private void SpawnDecor(GenerationProgress progress, GameConfiguration configuration)
	{
		SetSpawn();

		Dictionary<Point16, OpenFlags> grasses = [];

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; j++)
			{
				OpenFlags flags = OpenExtensions.GetOpenings(i, j, false, false);
				Tile tile = Main.tile[i, j];

				if (flags != OpenFlags.None)
				{
					if (j < Main.worldSurface && tile.HasTile && tile.TileType is TileID.Dirt or TileID.Pearlstone)
					{
						tile.WallType = WallID.None;
					}

					if (tile.TileType == TileID.Dirt)
					{
						tile.TileType = TileID.HallowedGrass;

						grasses.Add(new Point16(i, j), flags);
					}

					if (WorldGen.genRand.NextBool(2, 3))
					{
						Tile.SmoothSlope(i, j, false, false);
					}
				}
			}
		}

		foreach (KeyValuePair<Point16, OpenFlags> item in grasses)
		{
			DecorateGrass(item.Key, item.Value);
		}
	}

	private static void DecorateGrass(Point16 position, OpenFlags flags)
	{
		if (flags.HasFlag(OpenFlags.Above))
		{
			if (WorldGen.genRand.NextBool(10))
			{
				WorldGen.PlaceTile(position.X, position.Y - 1, TileID.Saplings);

				if (!WorldGen.GrowTree(position.X, position.Y - 1))
				{
					WorldGen.KillTile(position.X, position.Y - 1);
				}
			}
			else if (!WorldGen.genRand.NextBool(3))
			{
				WorldGen.PlaceTile(position.X, position.Y - 1, TileID.HallowedPlants);
			}
		}
	}

	private static void SetSpawn()
	{
		int x = LeftSpawn ? 90 : Main.maxTilesX - 90;
		int y = 80;

		while (!WorldGen.SolidOrSlopedTile(x, y))
		{
			y++;
		}

		y -= 5;

		Main.spawnTileX = x;
		Main.spawnTileY = y;
	}

	private void SpawnBaseTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		LeftSpawn = Main.rand.NextBool();
		Main.rockLayer = (int)(Height * 0.7f);
		Main.worldSurface = (int)(Height * 0.7f) - 50;

		Vector2 circleCenter = new(LeftSpawn ? 60 : Main.maxTilesX - 60, 60);
		FastNoiseLite noise = new();
		noise.SetFrequency(0.1f);

		FastNoiseLite stoneNoise = new();
		stoneNoise.SetFrequency(0.045f);

		FastNoiseLite stoneNoise2 = new();
		stoneNoise2.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
		stoneNoise2.SetFrequency(0.1f);

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; j++)
			{
				float dist = ModDistance(new Vector2(i, j), circleCenter);
				float cutoff = 650 + 10f * noise.GetNoise((new Vector2(i, j).AngleTo(circleCenter) + MathHelper.Pi) * 40, 0);
				float stoneCutoff = 780 + 20f * noise.GetNoise((new Vector2(i, j).AngleTo(circleCenter) + MathHelper.Pi) * 50, 0);

				bool stone = (stoneNoise.GetNoise(i, j) * (stoneNoise2.GetNoise(i, j) + 0.4f)) > Utils.GetLerpValue(stoneCutoff, cutoff, dist, true) * 0.15f;

				if (dist > cutoff)
				{
					Tile tile = Main.tile[i, j];
					tile.HasTile = true;
					tile.TileType = stone ? TileID.Pearlstone : TileID.Dirt;
					tile.WallType = stone ? WallID.PearlstoneEcho : WallID.DirtUnsafe;
				}
			}
		}

		DigTunnel();
	}

	private void DigTunnel()
	{
		Vector2[] positions = Tunnel.GeneratePoints([new Vector2(LeftSpawn ? 200 : Main.maxTilesX - 200, 220),
			new Vector2(LeftSpawn ? 430 : Main.maxTilesX - 430, Height * 0.7f - 20), new Vector2(Width / 2, Height * 0.8f - 20)], 8, 6);

		FastNoiseLite noise = new(WorldGen._genRandSeed);
		HashSet<Point> crystals = [];

		foreach (Vector2 pos in positions)
		{
			AddSpaceAndCrystals(noise, crystals, pos);
		}

		positions = Tunnel.GeneratePoints([new Vector2(Width / 2, Height * 0.8f - 20),
			new Vector2(!LeftSpawn ? 400 : Main.maxTilesX - 400, Height * 0.7f), 
			new Vector2(!LeftSpawn ? 150 : Main.maxTilesX - 150, Height * 0.4f - 20)], 8, 6);

		foreach (Vector2 pos in positions)
		{
			AddSpaceAndCrystals(noise, crystals, pos);
		}

		positions = Tunnel.GeneratePoints([new Vector2(Width / 2, Height * 0.8f - 20),
			new Vector2(!LeftSpawn ? 400 : Main.maxTilesX - 400, Height * 0.725f), 
			new Vector2(!LeftSpawn ? 100 : Main.maxTilesX - 100, Height * 0.5f - 20)], 8, 6);

		foreach (Vector2 pos in positions)
		{
			AddSpaceAndCrystals(noise, crystals, pos);
		}

		positions = Tunnel.GeneratePoints([new Vector2(Width / 2, Height * 0.8f - 20),
			new Vector2(!LeftSpawn ? 400 : Main.maxTilesX - 400, Height * 0.75f), 
			new Vector2(!LeftSpawn ? 150 : Main.maxTilesX - 150, Height * 0.8f - 20)], 8, 6);

		foreach (Vector2 pos in positions)
		{
			AddSpaceAndCrystals(noise, crystals, pos);
		}

		foreach (Point crystal in crystals)
		{
			SpawnCrystalWall(crystal);
		}
	}

	private static void AddSpaceAndCrystals(FastNoiseLite noise, HashSet<Point> crystals, Vector2 pos)
	{
		TunnelDig(noise, pos);

		if (Main.rand.NextBool(20) && !crystals.Any(cry => Math.Abs(pos.X - cry.X) < 50))
		{
			crystals.Add(pos.ToPoint());
		}
	}

	private static void SpawnCrystalWall(Point pos)
	{
		float originalY = pos.Y;
		
		while (!WorldGen.SolidOrSlopedTile(Main.tile[pos]))
		{
			pos.Y--;

			if (pos.Y < 30)
			{
				break;
			}
		}

		if (pos.Y < 30)
		{
			return;
		}

		pos.Y -= 15;
		float bottomY = 0;
		List<(int, int)> crystalPositions = [];

		while (true)
		{
			int variant = Main.rand.Next(16);
			crystalPositions.Add((pos.Y, variant));

			if (variant < 9)
			{
				pos.Y += WorldGen.genRand.Next(2, 5);
			}

			if (WorldGen.SolidOrSlopedTile(Main.tile[pos]) && bottomY == 0 && pos.Y > originalY)
			{
				bottomY = pos.Y;
			}

			if (bottomY != 0 && pos.Y > bottomY + 15)
			{
				break;
			}
		}

		foreach ((int, int) pair in crystalPositions)
		{
			string str = "Assets/Structures/QueenSlimeDomain/Crystal_" + pair.Item2;
			StructureTools.PlaceByOrigin(str, new Point16(pos.X, pair.Item1), new Vector2(0.5f));
		}
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

	private static float ModDistance(Vector2 position, Vector2 circleCenter)
	{
		return MathF.Sqrt(MathF.Pow(position.X - circleCenter.X, 2) + MathF.Pow(position.Y - circleCenter.Y, 2) * 5);
	}

	public override void Update()
	{
		if (!BossSpawned)
		{
			bool canSpawnBoss = false;

			foreach (Player player in Main.ActivePlayers)
			{
				if (player.DistanceSQ(ArenaPos.ToWorldCoordinates()) < 1300 * 1300)
				{
					canSpawnBoss = true;
					break;
				}
			}

			if (canSpawnBoss)
			{
				BossSpawned = true;

				NPC.NewNPC(Entity.GetSource_NaturalSpawn(), ArenaPos.X * 16, ArenaPos.Y * 16, NPCID.QueenSlimeBoss);
			}
		}
		else
		{
			if (!NPC.AnyNPCs(NPCID.QueenSlimeBoss) && !PortalSpawned)
			{
				PortalSpawned = true;

				IEntitySource src = Entity.GetSource_NaturalSpawn();
				Projectile.NewProjectile(src, ArenaPos.ToWorldCoordinates(), Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);
			}
		}
	}
}
