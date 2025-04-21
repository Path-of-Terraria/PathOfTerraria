using PathOfTerraria.Common.ItemDropping;
using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.World;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.World.Generation.Tools;
using PathOfTerraria.Content.NPCs.Mapping.Forest.GrovetenderBoss;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Content.Tiles.Maps.Forest;
using PathOfTerraria.Core.Items;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.MappingAreas;

internal class ForestArea : MappingWorld, IOverrideOcean
{
	private enum StructureKind : byte
	{
		Cave,
		Shrine,
		Campsite,
		AncientTree,
		Arena,
		Count,
	}

	public const int FloorY = 180;

	private static bool LeftSpawn = false;
	private static Point BossSpawnLocation = Point.Zero;

	public override int Width => 1200 + 120 * Main.rand.Next(10);
	public override int Height => 290;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep), new PassLegacy("Terrain", GenerateTerrain), new PassLegacy("Structures", GenStructures),
		new PassLegacy("Detailing", AddDetails)];

	public override void OnEnter()
	{
		SubworldSystem.noReturn = true;
	}

	public override void Update()
	{
		TileEntity.UpdateStart();
		foreach (TileEntity te in TileEntity.ByID.Values)
		{
			te.Update();
		}

		TileEntity.UpdateEnd();

		bool hasPortal = false;

		foreach (Projectile projectile in Main.ActiveProjectiles)
		{
			if (projectile.type == ModContent.ProjectileType<ExitPortal>())
			{
				hasPortal = true;
				break;
			}
		}

		if (!hasPortal && ModContent.GetInstance<GrovetenderSystem>().GrovetenderWhoAmI == -1 && !NPC.AnyNPCs(ModContent.NPCType<Grovetender>()))
		{
			int npc = NPC.NewNPC(new EntitySource_SpawnNPC(), BossSpawnLocation.X, BossSpawnLocation.Y, ModContent.NPCType<Grovetender>());

			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc);
			}
		}
	}

	private void GenStructures(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Structures");

		HashSet<StructureKind> structures = [];

		int arenaX = LeftSpawn ? Main.maxTilesX - 160 : 160;
		TryPlaceStructureAt(structures, arenaX, FloorY, StructureKind.Arena, 1, new Vector2(0.5f, 1), 45);
		BossSpawnLocation = new Point((arenaX - 2) * 16, (FloorY + 38) * 16);
		int attempts = 0;

		while (true)
		{
			if (structures.Count >= (int)StructureKind.Count * 1.8f)
			{
				break;
			}

			attempts++;

			progress.Set(attempts / 10000f);

			if (attempts > 10000)
			{
				break;
			}

			int x = WorldGen.genRand.Next(150, Main.maxTilesX - 150);
			int y = WorldGen.genRand.Next(FloorY - 10, FloorY + 5);

			if (!WorldGen.SolidTile(x, y))
			{
				continue;
			}

			if (OpenExtensions.GetOpenings(x, y).HasFlag(OpenFlags.Above))
			{
				TryPlaceStructureAt(structures, x, y, StructureKind.Cave, 3, new Vector2(0.5f, 0), -4);
			}
			else
			{
				StructureKind kind;

				if (structures.Count < (int)StructureKind.Count - 1)
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
					TryPlaceStructureAt(structures, x, y, StructureKind.Campsite, 3, new Vector2(0.5f, 1), 4);
				}
				else if (kind == StructureKind.AncientTree)
				{
					TryPlaceStructureAt(structures, x, y, StructureKind.AncientTree, 3, new Vector2(0.5f, 1), 7);
				}
			}
		}
	}

	private static void TryPlaceStructureAt(HashSet<StructureKind> structures, int x, int y, StructureKind type, int max, Vector2 origin, int offsetY = 0)
	{
		string path = $"Assets/Structures/MapAreas/ForestArea/{type}_{WorldGen.genRand.Next(max)}";
		bool isShrine = false;

		if (WorldGen.genRand.NextBool(5) && type != StructureKind.Arena && type != StructureKind.Cave)
		{
			path = $"Assets/Structures/MapAreas/ForestArea/SpecialShrine_{WorldGen.genRand.Next(5)}";
			isShrine = true;
		}

		Point16 size = StructureTools.GetSize(path);

		if (GenVars.structures.CanPlace(new Rectangle(x - (int)(size.X * origin.X), y - (int)(size.Y * origin.Y) + offsetY, size.X, size.Y)))
		{
			y += StructureTools.AverageHeights(x, y, size.X, 2000, 2, out bool valid, [], []);

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
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.PopulatingWorld");

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
						int type = WorldGen.genRand.NextBool(12) ? ModContent.TileType<Runestone>() : TileID.Stone;

						if (!WorldGen.genRand.NextBool(60) || !SpawnBoulder(i, j, type))
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

			progress.Set(i / (float)Main.maxTilesX);
		}

		GenerationUtilities.ManuallyPopulateChests();
		ShrineFunctionality.PopulateShrines();

		int grassIndex = 0;

		foreach (Point16 pos in grasses)
		{
			progress.Set(grassIndex++ / (float)grasses.Count);
			GrowGrassDecor(pos);
		}

		grassIndex = 0;

		foreach (Point16 pos in grasses)
		{
			progress.Set(grassIndex++ / (float)grasses.Count);
			AddGrassWalls(pos);
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

				bool canSmooth = GenVars.structures.CanPlace(new Rectangle(i, j, 1, 1));

				if (!canSmooth)
				{
					canSmooth = !(tile.TileType is TileID.GrayBrick or TileID.LivingWood);
				}

				if (tile.HasTile && canSmooth)
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

	private static void GrowGrassDecor(Point16 pos)
	{
		OpenFlags flags = OpenExtensions.GetOpenings(pos.X, pos.Y, false, false);

		if (flags.HasFlag(OpenFlags.Above))
		{
			if (WorldGen.genRand.NextBool(3))
			{
				WorldGen.GrowEpicTree(pos.X, pos.Y);
			}
			else if (WorldGen.genRand.NextBool(20))
			{
				int style = WorldGen.genRand.NextBool(2, 3) ? 1 : 0;
				WorldGen.PlaceTile(pos.X, pos.Y - 1, TileID.DyePlants, true, true, -1, style);

				if (style == 1) // Green mushroom
				{
					Tile tile = Main.tile[pos.X, pos.Y - 1];
					tile.TileColor = WorldGen.genRand.NextBool(3) ? PaintID.None : PaintID.RedPaint;
				}
			}
			else if (WorldGen.genRand.NextBool(40))
			{
				WorldGen.PlaceTile(pos.X, pos.Y - 1, TileID.DyePlants, true, true, WorldGen.genRand.Next(8, 12));
			}
			else
			{
				WorldGen.PlaceTile(pos.X, pos.Y - 1, WorldGen.genRand.NextBool() ? TileID.Plants2 : TileID.Plants, true, true, -1);
			}
		}
	}

	private static void AddGrassWalls(Point16 pos)
	{
		OpenFlags flags = OpenExtensions.GetOpenings(pos.X, pos.Y, false, false);

		if (flags != OpenFlags.None && WorldGen.genRand.NextBool(7))
		{
			int count = WorldGen.genRand.Next(1, 5);

			for (int i = 0; i < count; ++i)
			{
				var position = pos.ToVector2();

				if (i != 0)
				{
					position += Main.rand.NextVector2Circular(1, 1);
					position.Y += Main.rand.NextFloat(1, 3);
				}

				GenPlacement.WallCircle(position, WorldGen.genRand.NextFloat(2, 4f), WallID.LivingLeaf);
			}
		}
	}

	public static bool SpawnBoulder(int i, int j, int type, int size = -1, bool isWall = false)
	{
		if (size == -1)
		{
			size = WorldGen.genRand.Next(7, 18);
		}

		j += 3;

		if (!GenVars.structures.CanPlace(new Rectangle(i - size, j - size, size * 2, size * 2)))
		{
			return false;
		}

		FastNoiseLite noise = new();
		noise.SetFrequency(0.015f);
		Ellipse.GenOval(new Vector2(i, j), size, WorldGen.genRand.NextFloat(MathHelper.Pi) + MathHelper.PiOver2, isWall, type, noise);
		return true;
	}

	private void GenerateTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");

		Main.worldSurface = 240;
		Main.rockLayer = 270;

		LeftSpawn = Main.rand.NextBool(2);
		Main.spawnTileX = LeftSpawn ? 70 : Main.maxTilesX - 70;

		FastNoiseLite noise = new(WorldGen._genRandSeed);
		noise.SetFrequency(0.2f);
		noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);

		HashSet<Point16> leafBlobs = [];
		HashSet<int> trees = [];

		for (int i = 0; i < Main.maxTilesX; ++i)
		{
			int yCutoff = (int)(FloorY + noise.GetNoise(i * 0.05f, 0) * 4);
			int leafYOffset = (int)(noise.GetNoise(i * 0.1f, 0) * 10);

			if (i > 200 && i < Main.maxTilesX - 200)
			{
				if (i == Main.maxTilesX / 10 * 3 || i == Main.maxTilesX / 10 * 7 && WorldGen.genRand.NextBool())
				{
					trees.Add(i);
				}
			}

			progress.Set(i / (float)Main.maxTilesX);

			for (int j = 0; j < Main.maxTilesY; ++j)
			{
				int id = -1;
				int wallId = -1;

				int leafCutoff = (int)(60 + noise.GetNoise(i, j) * 15 + leafYOffset);

				if (j < leafCutoff)
				{
					id = TileID.LeafBlock;

					if (WorldGen.genRand.NextBool(50) && j > leafCutoff - 5)
					{
						leafBlobs.Add(new(i, j));
					}
				}
				else if (j > yCutoff)
				{
					id = TileID.Dirt;
				}
				else if (!LeftSpawn && i < 50 || LeftSpawn && i > Main.maxTilesX - 50)
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

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.TreesAndLeaves");

		foreach (int x in trees)
		{
			string path = "Assets/Structures/MapAreas/ForestArea/Tree_" + WorldGen.genRand.Next(2);
			Point16 pos = StructureTools.PlaceByOrigin(path, new Point16(x, FloorY - 10), new Vector2(0.5f, 0.75f));
			Point16 size = StructureTools.GetSize(path);
			GenVars.structures.AddProtectedStructure(new Rectangle(pos.X + 20, pos.Y, size.X - 40, size.Y), 10);
		}

		foreach (Point16 pos in leafBlobs)
		{
			bool isWall = WorldGen.genRand.NextBool();
			int type = isWall ? WallID.LivingLeaf : TileID.LeafBlock;
			Ellipse.GenOval(pos.ToVector2(), WorldGen.genRand.NextFloat(10, 50), WorldGen.genRand.NextFloat(MathHelper.TwoPi), isWall, type, noise);
		}
	}

	public void OverrideOcean()
	{
		Main.bgStyle = 0;
		Main.curMusic = MusicID.OverworldDay;
	}
}
