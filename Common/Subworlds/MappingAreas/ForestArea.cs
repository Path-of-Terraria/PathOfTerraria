using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Tiles.Maps.Forest;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.MappingAreas;

internal class ForestArea : MappingWorld
{
	public override int Width => 800;
	public override int Height => 250;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep), new PassLegacy("Terrain", GenerateTerrain), new PassLegacy("Detailing", AddDetails)];

	private void AddDetails(GenerationProgress progress, GameConfiguration configuration)
	{
		for (int i = 0; i < Main.maxTilesX; ++i)
		{
			for (int j = 0; j < Main.maxTilesY; ++j)
			{
				if (!WorldGen.InWorld(i, j, 20))
				{
					continue;
				}

				Tile tile = Main.tile[i, j];

				if (tile.TileType == TileID.Dirt && tile.HasTile)
				{
					if (OpenExtensions.GetOpenings(i, j, false, false) != OpenFlags.None)
					{
						if (!SpawnBoulder(i, j))
						{
							tile.TileType = TileID.Grass;

							if (WorldGen.genRand.NextBool(4))
							{
								Tile.SmoothSlope(i, j, true);
							}
						}
					}
					else if (WorldGen.genRand.NextBool(350))
					{
						int type = WorldGen.genRand.NextBool(20) ? ModContent.TileType<Runestone>() : TileID.Stone;
						WorldGen.TileRunner(i, j, WorldGen.genRand.NextFloat(5, 18), WorldGen.genRand.Next(6, 20), type);

						tile.WallType = WallID.Dirt;
					}
				}
			}
		}
		
		for (int i = 0; i < Main.maxTilesX; ++i)
		{
			for (int j = 0; j < Main.maxTilesY; ++j)
			{
				if (!WorldGen.InWorld(i, j, 20))
				{
					continue;
				}

				Tile tile = Main.tile[i, j];

				if (tile.HasTile)
				{
					Tile.SmoothSlope(i, j, true);
				}
			}
		}
	}

	private static bool SpawnBoulder(int i, int j)
	{
		if (!WorldGen.genRand.NextBool(150))
		{
			return false;
		}

		int size = Main.rand.Next(4, 9);
		int type = WorldGen.genRand.NextBool(20) ? ModContent.TileType<Runestone>() : TileID.Stone;

		FastNoiseLite noise = new FastNoiseLite();
		noise.SetFrequency(0.2f);

		for (int x = i - size * 2; x < i + size * 2; x++)
		{
			for (int y = j - size * 2; y < j + size * 2; y++)
			{
				Tile tile = Main.tile[x, y];
				float off = MathF.Pow(noise.GetNoise(i * 200, j * 200), 2);

				if (Vector2.DistanceSquared(new Vector2(i, j), new Vector2(x, y)) < size * size * off)
				{
					tile.HasTile = true;
					tile.TileType = (ushort)type;
				}
			}
		}

		return true;
	}

	private void GenerateTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		Main.worldSurface = 200;
		Main.rockLayer = 220;

		FastNoiseLite noise = new(WorldGen._genRandSeed);
		noise.SetFrequency(0.2f);
		noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);

		for (int i = 0; i < Main.maxTilesX; ++i)
		{
			int yCutoff = (int)(150 + noise.GetNoise(i * 0.05f, 0) * 4);
			int leafYOffset = (int)(noise.GetNoise(i * 0.1f, 0) * 10);

			for (int j = 0; j < Main.maxTilesY; ++j)
			{
				int id = -1;
				int wallId = -1;

				if (j < 60 + noise.GetNoise(i, j) * 15 + leafYOffset)
				{
					id = TileID.LeafBlock;
				}
				else if (j > yCutoff)
				{
					id = TileID.Dirt;
				}
				else if (i < 50 || i > Width - 50)
				{
					id = TileID.LivingWood;
				}

				if (j < 60 + noise.GetNoise(i, j + 600) * 15)
				{
					wallId = WallID.LivingLeaf;
				}

				Tile tile = Main.tile[i, j];

				if (id != -1)
				{
					tile.HasTile = true;
					tile.TileType = (ushort)id;
				}

				if (wallId != -1)
				{
					tile.WallType = (ushort)wallId;
				}
			}
		}
	}

	public class ForestScene : ModSystem
	{
		public override void ModifyLightingBrightness(ref float scale)
		{
			if (SubworldSystem.Current is ForestArea)
			{
				scale *= 0.7f;
			}
		}

		public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
		{
			if (SubworldSystem.Current is ForestArea)
			{
				tileColor = new Color(180, 70, 200);
				backgroundColor = new Color(160, 60, 180);
			}
		}
	}
}
