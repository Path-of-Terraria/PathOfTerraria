using PathOfTerraria.Common.ItemDropping;
using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.World;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.World.Passes;
using PathOfTerraria.Content.Tiles.Maps.Forest;
using PathOfTerraria.Core.Items;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.MappingAreas;

internal class ForestArea : MappingWorld
{
	private enum StructureKind : byte
	{
		Cave,
		Shrine,
		Campsite,
		//AncientTree,
		Arena,
		Count,
	}

	public const int FloorY = 180;

	private static bool LeftSpawn = false;
	private static int LastTreeX = 0;

	public override int Width => 1200;
	public override int Height => 330;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep), new PassLegacy("Terrain", GenerateTerrain), new PassLegacy("Structures", GenStructures),
		new PassLegacy("Detailing", AddDetails)];

	private void GenStructures(GenerationProgress progress, GameConfiguration configuration)
	{
		HashSet<StructureKind> structures = [];

		TryPlaceStructureAt(structures, LeftSpawn ? Width - 160 : 160, FloorY, StructureKind.Arena, 1, new Vector2(0.5f, 1), 45);
		int attempts = 0;

		while (true)
		{
			if (structures.Count >= (int)StructureKind.Count * 1.8f)
			{
				break;
			}

			attempts++;

			if (attempts > 20000)
			{
				break;
			}

			int x = WorldGen.genRand.Next(150, Width - 150);
			int y = WorldGen.genRand.Next(FloorY - 40, FloorY + 20);

			if (!WorldGen.SolidTile(x, y))
			{
				continue;
			}

			if (OpenExtensions.GetOpenings(x, y).HasFlag(OpenFlags.Above) && (!structures.Contains(StructureKind.Cave) || structures.Count >= (int)StructureKind.Count))
			{
				TryPlaceStructureAt(structures, x, y, StructureKind.Cave, 3, new Vector2(0.5f, 0), -4);
			}
			else
			{
				StructureKind kind;

				if (structures.Count < (int)StructureKind.Count)
				{
					do
					{
						kind = (StructureKind)WorldGen.genRand.Next((int)StructureKind.Count);
					} while (structures.Contains(kind) || kind == StructureKind.Arena);
				}
				else
				{
					do
					{
						kind = (StructureKind)WorldGen.genRand.Next((int)StructureKind.Count);
					} while (kind == StructureKind.Arena);
				}

				if (kind == StructureKind.Shrine)
				{
					TryPlaceStructureAt(structures, x, y, StructureKind.Shrine, 3, new Vector2(0.5f, 1), 4);
				}
				else if (kind == StructureKind.Campsite)
				{
					TryPlaceStructureAt(structures, x, y, StructureKind.Campsite, 2, new Vector2(0.5f, 1), 4);
				}
			}
		}
	}

	private static void TryPlaceStructureAt(HashSet<StructureKind> structures, int x, int y, StructureKind type, int max, Vector2 origin, int offsetY = 0)
	{
		string path = $"Assets/Structures/MapAreas/ForestArea/{type}_{WorldGen.genRand.Next(max)}";
		Point16 size = StructureTools.GetSize(path);

		y += StructureTools.AverageHeights(x, y, size.X, 2000, 2, out bool valid, [], []);

		if (GenVars.structures.CanPlace(new Rectangle(x - (int)(size.X * origin.X), y - (int)(size.Y * origin.Y) + offsetY, size.X, size.Y)))
		{
			Point16 pos = StructureTools.PlaceByOrigin(path, new Point16(x, y + offsetY), origin);

			structures.Add(type);
			GenVars.structures.AddProtectedStructure(new Rectangle(pos.X, pos.Y, size.X, size.Y), 10);

			for (int i = pos.X; i < pos.X + size.X; ++i)
			{
				int newY = pos.Y + size.Y;

				while (newY < FloorY + 10)
				{
					Tile tile = Main.tile[i, newY];
					
					if (!tile.HasTile)
					{
						tile.HasTile = true;
						tile.TileType = TileID.Dirt;
					}

					newY++;
				}
			}
		}
	}

	private void AddDetails(GenerationProgress progress, GameConfiguration configuration)
	{
		HashSet<Point16> grasses = [];

		for (int i = 0; i < Main.maxTilesX; ++i)
		{
			for (int j = 0; j < Main.maxTilesY; ++j)
			{
				if (!WorldGen.InWorld(i, j, 20))
				{
					continue;
				}

				Tile tile = Main.tile[i, j];
				OpenFlags flags = OpenExtensions.GetUnsolidOpenings(i, j, false, false);

				if (tile.TileType == TileID.Dirt && tile.HasTile)
				{
					if (flags != OpenFlags.None)
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
					else 
					{
						if (WorldGen.genRand.NextBool(350) && GenVars.structures.CanPlace(new Rectangle(i - 6, j - 6, 12, 12)))
						{
							int type = WorldGen.genRand.NextBool(20) ? ModContent.TileType<Runestone>() : TileID.Stone;
							WorldGen.TileRunner(i, j, WorldGen.genRand.NextFloat(5, 18), WorldGen.genRand.Next(6, 20), type);
						}

						tile.WallType = WallID.Dirt;
					}
				}

				if (tile.TileType == TileID.Grass)
				{
					if (flags == OpenFlags.None)
					{
						tile.TileType = TileID.Dirt;
					}
					else
					{
						grasses.Add(new Point16(i, j));
					}
				}
			}
		}

		foreach (Point16 pos in grasses)
		{
			GrowStuffOnGrass(pos);
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

		for (int i = 0; i < Main.maxChests; ++i)
		{
			Chest chest = Main.chest[i];

			if (chest is null)
			{
				continue;
			}

			Tile tile = Main.tile[chest.x, chest.y];

			if (tile.HasTile && TileID.Sets.BasicChest[tile.TileType])
			{
				for (int k = 0; k < 3; ++k)
				{
					ItemDatabase.ItemRecord drop = DropTable.RollMobDrops(PoTItemHelper.PickItemLevel(), 1f, random: WorldGen.genRand);

					chest.item[k] = new Item(drop.ItemId, drop.Item.stack);
				}
			}
		}
	}

	private static void GrowStuffOnGrass(Point16 pos)
	{
		OpenFlags flags = OpenExtensions.GetOpenings(pos.X, pos.Y, false, false);

		if (flags.HasFlag(OpenFlags.Above))
		{
			if (WorldGen.genRand.NextBool(3))
			{
				WorldGen.GrowEpicTree(pos.X, pos.Y);
			}
		}
	}

	private static bool SpawnBoulder(int i, int j)
	{
		int size = Main.rand.Next(7, 18);
		j += 3;

		if (!WorldGen.genRand.NextBool(60) || !GenVars.structures.CanPlace(new Rectangle(i - size, j - size, size * 2, size * 2)))
		{
			return false;
		}

		int type = WorldGen.genRand.NextBool(12) ? ModContent.TileType<Runestone>() : TileID.Stone;

		FastNoiseLite noise = new();
		noise.SetFrequency(0.015f);

		Ellipse.GenOval(new Vector2(i, j), size, WorldGen.genRand.NextFloat(MathHelper.Pi) + MathHelper.PiOver2, false, type, noise);
		return true;
	}

	private void GenerateTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		Main.worldSurface = 240;
		Main.rockLayer = 270;

		LeftSpawn = WorldGen.genRand.NextBool(2);
		Main.spawnTileX = LeftSpawn ? 60 : Width - 60;

		FastNoiseLite noise = new(WorldGen._genRandSeed);
		noise.SetFrequency(0.2f);
		noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);

		HashSet<Point16> leafBlobs = [];
		HashSet<int> trees = [];

		for (int i = 0; i < Main.maxTilesX; ++i)
		{
			int yCutoff = (int)(FloorY + noise.GetNoise(i * 0.05f, 0) * 4);
			int leafYOffset = (int)(noise.GetNoise(i * 0.1f, 0) * 10);

			if (i > 200 && i < Width - 200)
			{
				if (Math.Abs(LastTreeX - i) > (Width - 200) / 3)
				{
					trees.Add(i);
				}
			}

			for (int j = 0; j < Main.maxTilesY; ++j)
			{
				int id = -1;
				int wallId = -1;

				if (j < 60 + noise.GetNoise(i, j) * 15 + leafYOffset)
				{
					id = TileID.LeafBlock;

					if (WorldGen.genRand.NextBool(50))
					{
						leafBlobs.Add(new(i, j));
					}
				}
				else if (j > yCutoff)
				{
					id = TileID.Dirt;
				}
				else if (!LeftSpawn && i < 50 || LeftSpawn && i > Width - 50)
				{
					id = TileID.LivingWood;
				}

				if (j < 60 + noise.GetNoise(i, j + 600) * 15 || j < 60 + noise.GetNoise(i, j + 1200) * 15 || j < 60 + noise.GetNoise(i, j + 1800) * 15)
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

		foreach (int x in trees)
		{
			StructureTools.PlaceByOrigin("Assets/Structures/MapAreas/ForestArea/Tree_0", new Point16(x, FloorY), new Vector2(0.5f, 0.75f));
		}

		foreach (Point16 pos in leafBlobs)
		{
			bool isWall = WorldGen.genRand.NextBool();
			int type = isWall ? WallID.LivingLeaf : TileID.LeafBlock;
			Ellipse.GenOval(pos.ToVector2(), WorldGen.genRand.NextFloat(10, 50), WorldGen.genRand.NextFloat(MathHelper.TwoPi), isWall, type, noise);
		}
	}

	public class ForestScene : ModSystem
	{
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
