using PathOfTerraria.Common.World.Generation;
using System.Collections.Generic;
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

	private static bool LeftSpawn = false;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Base Terrain", SpawnBaseTerrain),
		new PassLegacy("Structures", SpawnStructures),
		new PassLegacy("Decor", SpawnDecor)];

	private void SpawnStructures(GenerationProgress progress, GameConfiguration configuration)
	{
		int x = Main.maxTilesX / 2 + (LeftSpawn ? 70 : -70);
		int y = 160;

		StructureTools.PlaceByOrigin("Assets/Structures/QueenSlimeDomain/Arena_0", new Point16(x, y), new Vector2(0.5f, 0.3f));
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
			new	Vector2(Width / 2, Height * 0.8f - 20), new(LeftSpawn ? Main.maxTilesX - 150 : 150, 300)], 8, 6);

		FastNoiseLite noise = new(WorldGen._genRandSeed);

		foreach (Vector2 pos in positions)
		{
			float mul = 0.8f + MathF.Abs(noise.GetNoise(pos.X, pos.Y));
			Digging.CircleOpening(pos + WorldGen.genRand.NextVector2Circular(4, 4), 8 * mul);
			Digging.CircleOpening(pos, WorldGen.genRand.Next(3, 7) * mul);

			if (WorldGen.genRand.NextBool(3, 5))
			{
				WorldGen.digTunnel(pos.X, pos.Y, 0, 0, 5, (int)(WorldGen.genRand.NextFloat(5, 9) * mul));
			}
		}
	}

	private static float ModDistance(Vector2 position, Vector2 circleCenter)
	{
		return MathF.Sqrt(MathF.Pow(position.X - circleCenter.X, 2) + MathF.Pow(position.Y - circleCenter.Y, 2) * 5);
	}
}
