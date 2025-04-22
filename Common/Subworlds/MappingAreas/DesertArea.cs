using PathOfTerraria.Common.ItemDropping;
using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.World.Generation.Tools;
using PathOfTerraria.Content.NPCs.Mapping.Desert;
using PathOfTerraria.Content.NPCs.Mapping.Desert.SunDevourer;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Core.Items;
using SubworldLibrary;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Events;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.MappingAreas;

internal class DesertArea : MappingWorld, IOverrideOcean
{
	public const int FloorY = 400;

	private static bool LeftSpawn = false;
	private static Point16 BossSpawnLocation = Point16.Zero;
	private static int SandstormTimer = 0;

	public override int Width => 2600 + 150 * Main.rand.Next(3);
	public override int Height => 600;
	public override int[] WhitelistedMiningTiles => [TileID.CrackedBlueDungeonBrick];
	public override (int time, bool isDay) ForceTime => ((int)Main.dayLength / 2, true);

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep), new PassLegacy("Terrain", GenerateTerrain), 
		new PassLegacy("Decor", GenerateDecor)];

	public override void Load()
	{
		On_Player.UpdateBiomes += ForceActiveSandstormBiomeInDesert;
		On_Sandstorm.HandleEffectAndSky += ForceActiveSandstormInDesert;
	}

	private void ForceActiveSandstormInDesert(On_Sandstorm.orig_HandleEffectAndSky orig, bool toState)
	{
		if (SubworldSystem.Current is DesertArea && Sandstorm.Happening)
		{
			toState = true;
		}

		orig(toState);
	}

	private void ForceActiveSandstormBiomeInDesert(On_Player.orig_UpdateBiomes orig, Player self)
	{
		orig(self);

		if (SubworldSystem.Current is DesertArea && Sandstorm.Happening)
		{
			self.ZoneSandstorm = true;
		}
	}

	private void GenerateDecor(GenerationProgress progress, GameConfiguration configuration)
	{
		List<Point16> boulders = [];

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; ++j)
			{
				Tile tile = Main.tile[i, j];
				OpenFlags flags = OpenExtensions.GetOpenings(i, j);

				if (tile.HasTile && WorldGen.genRand.NextBool(12) && flags.HasFlag(OpenFlags.Above))
				{
					boulders.Add(new Point16(i, j));
				}
			}

			progress.Set(i / (float)Main.maxTilesX);
		}

		for (int i = 0; i < boulders.Count; i++)
		{
			Point16 item = boulders[i];
			SpawnBoulder(item.X, item.Y);

			progress.Set(i / (float)boulders.Count);
		}

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Tunnels");

		HashSet<int> tunnelXPositions = [];
		DigTunnels(tunnelXPositions);
		PlaceColumns(tunnelXPositions);
		AddTileVariance();

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.PopulatingWorld");

		SpawnStructures();

		for (int i = 20; i < Main.maxTilesX - 20; ++i)
		{
			for (int j = 20; j < Main.maxTilesY - 40; ++j)
			{
				if (GenVars.structures.CanPlace(new Rectangle(i, j, 1, 1), 2))
				{
					Tile.SmoothSlope(i, j, false, false);
				}
			}

			progress.Set(i / (float)Main.maxTilesX);
		}

		progress.Set(0);
		GenerationUtilities.ManuallyPopulateChests();
		ShrineFunctionality.PopulateShrines();

		progress.Set(0.33f);
		DecorateSand();

		progress.Set(0.67f);
		PopulateChests();

		progress.Set(1);
		AddTrappers();
	}

	private static void AddTrappers()
	{
		int count = 9;

		while (count > 0)
		{
			int x = WorldGen.genRand.Next(400, Main.maxTilesX - 400);
			int y = WorldGen.genRand.Next(40, Main.maxTilesY - 50);// FindYBelow(x, 50);
			Vector2 left = new Vector2(x - 3, y).ToWorldCoordinates(0, WorldGen.genRand.NextFloat(16));

			if (NoOpening(left, 6 * 16) && !Collision.SolidCollision(left - new Vector2(0, 20), 6 * 16, 16))
			{
				NPC.NewNPC(Entity.GetSource_NaturalSpawn(), x * 16, y * 16 - 16, ModContent.NPCType<AntlionTrapper>());
				count--;
			}
		}
	}

	private static bool NoOpening(Vector2 position, int width)
	{
		int leftX = (int)(position.X / 16f) - 1;
		int rightX = (int)((position.X + width) / 16f) + 2;
		int y = (int)(position.Y / 16f);

		leftX = Utils.Clamp(leftX, 0, Main.maxTilesX - 1);
		rightX = Utils.Clamp(rightX, 0, Main.maxTilesX - 1);

		for (int i = leftX; i < rightX; i++)
		{
			if (!WorldGen.SolidOrSlopedTile(i, y))
			{
				return false;
			}
		}

		return true;
	}

	private static void DecorateSand()
	{
		for (int i = 20; i < Main.maxTilesX - 20; ++i)
		{
			for (int j = 20; j < Main.maxTilesY - 20; ++j)
			{
				Tile tile = Main.tile[i, j];
				OpenFlags flags = OpenExtensions.GetOpenings(i, j);

				if (flags.HasFlag(OpenFlags.Above) && tile.HasTile)
				{
					if (WorldGen.genRand.NextBool(10) && tile.TileType == TileID.Sand && i > 180 && i < Main.maxTilesX - 180)
					{
						WorldGen.PlantCactus(i, j);
					}
					else if (WorldGen.genRand.NextBool(40))
					{
						int type = WorldGen.genRand.NextBool() ? WorldGen.genRand.Next(29, 35) : WorldGen.genRand.Next(52, 55);
						WorldGen.PlaceObject(i, j - 1, TileID.LargePiles2, true, type);
					}
					else if (WorldGen.genRand.NextBool(26))
					{
						if (WorldGen.genRand.NextBool())
						{
							WorldGen.PlaceSmallPile(i, j - 1, WorldGen.genRand.Next(42, 47), 1);
						}
						else if (tile.TileType == TileID.Sand)
						{
							WorldGen.PlaceSmallPile(i, j - 1, WorldGen.genRand.Next(62, 65), 1);
						}
					}
					else if (WorldGen.genRand.NextBool(12))
					{
						WorldGen.PlaceSmallPile(i, j - 1, WorldGen.genRand.Next(54, 60), 0);
					}
					else if (WorldGen.genRand.NextBool(160) && tile.TileType == TileID.Sand && GenVars.structures.CanPlace(new Rectangle(i, j, 1, 1), 2))
					{
						float x = i;
						int length = WorldGen.genRand.Next(8, 18);

						for (int y = 0; y < length; ++y)
						{
							Tile hole = Main.tile[(int)x, j + y];

							if (WorldGen.genRand.NextBool())
							{
								hole.Clear(TileDataType.All ^ TileDataType.Wall);
							}
							else
							{
								hole.TileType = TileID.Sandstone;
							}

							x += WorldGen.genRand.NextFloat(-0.5f, 0.5f);
						}
					}
				}
			}
		}
	}

	private static void AddTileVariance()
	{
		FastNoiseLite noise = new(WorldGen._genRandSeed);
		noise.SetFrequency(0.005f);
		noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);

		for (int i = 20; i < Main.maxTilesX - 20; ++i)
		{
			for (int j = 20; j < Main.maxTilesY - 20; ++j)
			{
				Tile tile = Main.tile[i, j];
				OpenFlags flags = OpenExtensions.GetOpenings(i, j);

				if (tile.TileType == TileID.Sand && flags.HasFlag(OpenFlags.Below))
				{
					PlaceSandstone(i, j, noise);
				}
				else if (tile.TileType != TileID.Sandstone && flags.HasFlag(OpenFlags.Above))
				{
					PlaceSand(i, j, noise);
				}
			}
		}
	}

	private static void PlaceSandstone(int i, int j, FastNoiseLite noise)
	{
		int depth = (int)(6 + noise.GetNoise(i, 1800) * 2);

		for (int y = j; y > j - depth; --y)
		{
			Tile tile = Main.tile[i, y];

			if (tile.HasTile && tile.TileType == TileID.Sand)
			{
				tile.TileType = TileID.Sandstone;
			}
		}
	}

	private static void PlaceSand(int i, int j, FastNoiseLite noise)
	{
		int depth = (int)(12 + noise.GetNoise(i, 9000) * 12);

		for (int y = j; y < j + depth; ++y)
		{
			Tile tile = Main.tile[i, y];
			
			if (tile.HasTile && tile.TileType != TileID.Sandstone)
			{
				tile.TileType = TileID.Sand;
			}
		}
	}

	private static void DigTunnels(HashSet<int> xPositions)
	{
		FastNoiseLite noise = new(WorldGen._genRandSeed);
		noise.SetFrequency(0.02f);
		noise.SetFractalGain(0.02f);
		List<int> centres = [];

		for (int i = 0; i < 3; ++i)
		{
			int startX = WorldGen.genRand.Next(500, Main.maxTilesX - 500);
			int endX = startX + WorldGen.genRand.Next(180, 220) * (WorldGen.genRand.NextBool() ? -1 : 1);
			int centerX = (startX + endX) / 2;

			while (centres.Any(x => Math.Abs(centerX - x) < 300))
			{
				startX = WorldGen.genRand.Next(500, Main.maxTilesX - 500);
				endX = startX + WorldGen.genRand.Next(180, 220) * (WorldGen.genRand.NextBool() ? -1 : 1);
				centerX = (startX + endX) / 2;
			}

			int startY = FindYBelow(startX, 20);
			int endY = FindYBelow(endX, 20);
			int min = Math.Min(startX, endX);
			int max = Math.Max(startX, endX);
			int dif = max - min;
			int tunnelCenterX = Math.Clamp(centerX + WorldGen.genRand.Next(-dif / 2, dif / 2), min + 10, max - 10);

			Vector2[] tunnel = Tunnel.GeneratePoints([new(startX, startY), new(tunnelCenterX, (startY + endY) / 2 + WorldGen.genRand.Next(50, 90)), 
				new(endX, endY)], 20, 2, 0.2f);

			foreach (Vector2 pos in tunnel)
			{
				TunnelDig(noise, pos);
			}

			centres.Add(centerX);

			for (int x = min; x < max; ++x)
			{
				xPositions.Add(x);
			}
		}
	}

	private static void PlaceColumns(HashSet<int> xPositions)
	{
		int lastX = 0;
		FastNoiseLite noise = new(WorldGen._genRandSeed);
		noise.SetFrequency(0.08f);

		var tempList = xPositions.ToList();
		tempList.Sort();
		xPositions = new HashSet<int>(tempList);

		foreach (int x in xPositions)
		{
			if (WorldGen.genRand.NextBool(16) && lastX < x - 20)
			{
				for (int i = x - 3; i < x + 3; ++i)
				{
					if (!xPositions.Contains(i))
					{
						continue;
					}

					PlaceSandstoneColumn(i, noise, i != x - 3 && i != x + 2);
				}

				lastX = x;
			}
		}
	}

	private static void PlaceSandstoneColumn(int x, FastNoiseLite noise, bool hasWalls)
	{
		int y = Main.maxTilesY - 60;

		while (WorldGen.SolidOrSlopedTile(x, y))
		{
			y--;
		}

		int baseY = y;

		while (!WorldGen.SolidOrSlopedTile(x, y))
		{
			y--;

			if (y <= 10)
			{
				return;
			}
		}

		int targetY = y;
		y = baseY + 1;

		for (int j = y; j < y + 4; ++j)
		{
			Tile tile = Main.tile[x, j];
			tile.TileType = TileID.SandStoneSlab;
		}

		if (hasWalls)
		{
			while (y > targetY)
			{

				Tile tile = Main.tile[x, y];
				tile.WallType = noise.GetNoise(x, y) <= 0.08f ? WallID.ObsidianBackUnsafe : WallID.SandstoneBrick;

				if (noise.GetNoise(x, y + 2039) > 0.25f)
				{
					tile.WallType = WallID.Sandstone;
				}

				y--;
			}
		}
		else
		{
			y = targetY;
		}

		for (int j = y; j > y - 4; --j)
		{
			Tile tile = Main.tile[x, j];
			tile.TileType = TileID.SandStoneSlab;
			tile.WallType = noise.GetNoise(x, j) <= 0.08f ? WallID.ObsidianBackUnsafe : WallID.SandstoneBrick;

			if (noise.GetNoise(x, j + 2039) > 0.25f)
			{
				tile.WallType = WallID.Sandstone;
			}
		}
	}

	private static void TunnelDig(FastNoiseLite noise, Vector2 pos)
	{
		float mul = 0.5f + MathF.Abs(noise.GetNoise(pos.X, pos.Y));
		Digging.CircleOpening(pos + WorldGen.genRand.NextVector2Circular(4, 4), 8 * mul);
		Digging.CircleOpening(pos, WorldGen.genRand.Next(3, 7) * mul);

		if (WorldGen.genRand.NextBool(3, 25))
		{
			WorldGen.digTunnel(pos.X, pos.Y, 0, 0, 5, (int)(WorldGen.genRand.NextFloat(5, 9) * mul));
		}
	}

	private static int FindYBelow(int x, int y)
	{
		while (!WorldGen.SolidOrSlopedTile(x, y))
		{
			y++;
		}

		return y;
	}

	private static void PopulateChests()
	{
		WeightedRandom<(int type, Range stackRange)> miscChestLoot = new();
		miscChestLoot.Add((ItemID.DesertFossil, 9..30), 1f);
		miscChestLoot.Add((ItemID.FossilOre, 12..20), 0.5f);
		miscChestLoot.Add((ItemID.AncientBattleArmorMaterial, 1..2), 0.05f);
		miscChestLoot.Add((ItemID.DjinnLamp, 1..1), 0.0005f);
		miscChestLoot.Add((ItemID.Cactus, 20..60), 0.4f);
		miscChestLoot.Add((ItemID.AncientCloth, 2..5), 0.2f);
		miscChestLoot.Add((ItemID.DjinnsCurse, 2..5), 0.0005f);
		miscChestLoot.Add((ItemID.CatBast, 1..1), 0.05f);
		miscChestLoot.Add((ItemID.AncientHorn, 1..1), 0.005f);

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
				for (int k = 0; k < 5; ++k)
				{
					if (k < 3)
					{
						ItemDatabase.ItemRecord drop = DropTable.RollMobDrops(PoTItemHelper.PickItemLevel(), 1f, random: WorldGen.genRand);

						chest.item[k] = new Item(drop.ItemId, drop.Item.stack);
					}
					else
					{
						(int type, Range stackRange) = miscChestLoot.Get();
						chest.item[k] = new Item(type, Main.rand.Next(stackRange.Start.Value, stackRange.End.Value + 1));
					}
				}
			}
		}
	}

	private static void SpawnStructures()
	{
		PlaceArena();

		// Ruins, mostly embedded in sand
		int count = 5;

		while (count > 0)
		{
			Point16 pos = GetOpenAirRandomPosition();
			string structurePath = "Assets/Structures/MapAreas/DesertArea/Ruin_" + WorldGen.genRand.Next(4);
			Point16 structureSize = StructureTools.GetSize(structurePath);
			bool left = CanEmbedStructureIn(pos, structureSize);
			bool right = CanEmbedStructureIn(new Point16(pos.X - structureSize.X, pos.Y), structureSize);

			if (!left && !right)
			{
				continue;
			}

			float originX = 0;

			if (left && right)
			{
				originX = WorldGen.genRand.NextBool() ? 0 : 1;
			}
			else if (left)
			{
				originX = 1;
			}

			if (!GenVars.structures.CanPlace(new Rectangle(pos.X - (int)(structureSize.X * (1 - originX)), pos.Y, structureSize.X, structureSize.Y)))
			{
				continue;
			}

			pos = StructureTools.PlaceByOrigin(structurePath, pos, new Vector2(1 - originX, 1));
			GenVars.structures.AddProtectedStructure(new Rectangle(pos.X, pos.Y, structureSize.X, structureSize.Y), 10);
			count--;
		}

		// Oasis
		count = 2;
		int reps = 0;
		bool hasShrine = WorldGen.genRand.NextBool(ShrineFunctionality.ShrineDenominator);

		while (count > 0)
		{
			reps++;

			if (reps > 20000)
			{
				break;
			}

			Point16 pos = GetOpenAirRandomPosition();
			string structurePath = WorldGen.genRand.NextBool() 
				? "Assets/Structures/MapAreas/DesertArea/Oasis_" + WorldGen.genRand.Next(3)
				: "Assets/Structures/MapAreas/DesertArea/Obelisk_" + WorldGen.genRand.Next(2);

			bool isShrine = false;

			if (hasShrine)
			{
				isShrine = true;
				structurePath = "Assets/Structures/MapAreas/DesertArea/Shrine_" + WorldGen.genRand.Next(5);
			}

			Point16 structureSize = StructureTools.GetSize(structurePath);
			bool canPlace = CanPlaceStructureOn(pos, structureSize);

			if (!canPlace || !GenVars.structures.CanPlace(new Rectangle(pos.X - structureSize.X, pos.Y, structureSize.X, structureSize.Y)))
			{
				continue;
			}

			pos = StructureTools.PlaceByOrigin(structurePath, pos, new Vector2(0, 1));
			GenVars.structures.AddProtectedStructure(new Rectangle(pos.X, pos.Y, structureSize.X, structureSize.Y), 10);
			count--;

			if (isShrine)
			{
				hasShrine = false;
			}
		}
	}

	private static void PlaceArena()
	{
		string structure = "Assets/Structures/MapAreas/DesertArea/Arena";
		Point16 size = StructureTools.GetSize(structure);
		int x = LeftSpawn ? Main.maxTilesX - 140 - size.X : 140;
		int lowestY = 0;
		int lowestX = 0;
		float lowestOriginX = 0;

		for (int i = x; i < x + size.X; ++i)
		{
			int y = FindYBelow(i, 0);
			
			if (y > lowestY)
			{
				lowestY = y;
				lowestX = i;
				lowestOriginX = (i - x) / (float)size.X;
			}
		}

		Point16 pos = StructureTools.PlaceByOrigin(structure, new Point16(lowestX, lowestY), new Vector2(lowestOriginX, 1));
		GenVars.structures.AddProtectedStructure(new Rectangle(pos.X, pos.Y, size.X, size.Y), 10);
		BossSpawnLocation = pos + new Point16(size.X / 2, (int)(size.Y / 1.25f));
	}

	private static bool CanEmbedStructureIn(Point16 pos, Point16 structureSize)
	{
		for (int i = pos.X; i < pos.X + structureSize.X; ++i)
		{
			if (!WorldGen.SolidOrSlopedTile(i, pos.Y))
			{
				return false;
			}
		}

		return true;
	}

	private static bool CanPlaceStructureOn(Point16 pos, Point16 structureSize)
	{
		const int VerticalOffset = 20;

		for (int i = pos.X; i < pos.X + structureSize.X; ++i)
		{
			if (!WorldGen.SolidOrSlopedTile(i, pos.Y))
			{
				return false;
			}

			if (WorldGen.SolidOrSlopedTile(i, pos.Y - VerticalOffset) || Main.tile[i, pos.Y - VerticalOffset].WallType > 0)
			{
				return false;
			}
		}

		return true;
	}

	private static Point16 GetOpenAirRandomPosition()
	{
		while (true)
		{
			Point16 pos = new(WorldGen.genRand.Next(200, Main.maxTilesX - 200), WorldGen.genRand.Next(80, Main.maxTilesY - 80));

			if (Main.tile[pos].HasTile && Main.tile[pos].TileType == TileID.Sand && !Main.tile[pos.X, pos.Y - 1].HasTile)
			{
				return pos;
			}
		}
	}

	private static void SpawnBoulder(int i, int j)
	{
		bool isWall = false;
		ushort type = WorldGen.genRand.NextBool(4) ? TileID.HardenedSand : TileID.Sandstone;

		if (WorldGen.genRand.NextBool(2, 5))
		{
			isWall = true;
			type = WorldGen.genRand.NextBool(4) ? WallID.HardenedSand : WallID.Sandstone;
		}

		ForestArea.SpawnBoulder(i, j, type, WorldGen.genRand.Next(4, 30), isWall);
	}

	private void GenerateTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		const int MinHeight = 210;

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");

		Main.worldSurface = 240;
		Main.rockLayer = 270;

		LeftSpawn = Main.rand.NextBool(2);
		Main.spawnTileX = LeftSpawn ? 70 : Main.maxTilesX - 70;

		FastNoiseLite noise = new(WorldGen._genRandSeed);
		noise.SetFrequency(0.005f);
		noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);

		FastNoiseLite superNoise = new(WorldGen._genRandSeed);
		superNoise.SetFrequency(0.05f);
		superNoise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);

		float cutOffY = MinHeight;
		int start = 0;
		int end = Main.maxTilesX;

		if (!LeftSpawn)
		{
			start = Main.maxTilesX;
			end = 0;
		}

		int i = start;

		while (i != end)
		{
			float factor = i / (float)Main.maxTilesX;
			progress.Set(!LeftSpawn ? 1 - factor : factor);

			for (int j = 40; j < Main.maxTilesY; ++j)
			{
				Tile tile = Main.tile[i, j];
				
				if (j >= cutOffY)
				{
					tile.TileType = TileID.HardenedSand;
					tile.HasTile = true;

					if (i == Main.spawnTileX && j == (int)cutOffY + 1)
					{
						Main.spawnTileY = j - 4;
					}

					if (j >= cutOffY + 2.5f)
					{
						tile.WallType = superNoise.GetNoise(i, j) > 0.1f ? WallID.HardenedSand : WallID.Sandstone;
					}
				}
			}

			cutOffY -= (noise.GetNoise(i, 0) + 0.5f) * (noise.GetNoise(i + 9832, 0) + 0.5f) * (1 + superNoise.GetNoise(i, 0)) * 1.4f;

			int edge = i < 200 ? 40 : Main.maxTilesX - 40;
			float lerpValue = 0.01f;

			if (Math.Abs(edge - i) < 80)
			{
				lerpValue += Math.Abs(edge - i) / 40f * 0.05f;
			}

			cutOffY = MathHelper.Lerp(cutOffY, MinHeight, lerpValue);

			i += Math.Sign(end - i);
		}
	}

	public override void Update()
	{
		TileEntity.UpdateStart();
		foreach (TileEntity te in TileEntity.ByID.Values)
		{
			te.Update();
		}

		TileEntity.UpdateEnd();
		Wiring.UpdateMech();
		UpdateSandstorm();

		bool hasPortal = false;

		foreach (Projectile projectile in Main.ActiveProjectiles)
		{
			if (projectile.type == ModContent.ProjectileType<ExitPortal>())
			{
				hasPortal = true;
				break;
			}
		}

		if (!hasPortal && !NPC.AnyNPCs(ModContent.NPCType<SunDevourerNPC>()))
		{
			int x = BossSpawnLocation.X * 16;
			int y = BossSpawnLocation.Y * 16;
			int npc = NPC.NewNPC(new EntitySource_SpawnNPC(), x, y, ModContent.NPCType<SunDevourerNPC>(), 0, x, y - 40 * 16);

			Main.npc[npc].localAI[3] = y + 20 * 16;

			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc);
			}
		}
	}

	private static void UpdateSandstorm()
	{
		if (CanRunSandstorm())
		{
			if (SandstormTimer != 0 && Sandstorm.Happening)
			{
				Sandstorm.StopSandstorm();
				Main.windSpeedTarget = 0;
			}

			SandstormTimer = 0;
			return;
		}

		SandstormTimer++;
		int max = !Sandstorm.Happening ? 40 * 60 : 20 * 60;

		if (SandstormTimer > max)
		{
			if (!Sandstorm.Happening)
			{
				Sandstorm.StartSandstorm();
				Main.windSpeedTarget = Main.rand.NextFloat(1, 2);
			}
			else
			{
				Sandstorm.StopSandstorm();
				Main.windSpeedTarget = 0;
			}

			SandstormTimer = 0;
		}
	}

	private static bool CanRunSandstorm()
	{
		if (!Main.CurrentFrameFlags.AnyActiveBossNPC)
		{
			return true;
		}

		int bossWho = NPC.FindFirstNPC(ModContent.NPCType<SunDevourerNPC>());

		if (bossWho != -1)
		{
			NPC boss = Main.npc[bossWho];

			if (boss.ai[0] > 0)
			{
				return false;
			}
		}

		return true;
	}

	public void OverrideOcean()
	{
		Main.bgStyle = 2;
		Main.newMusic = MusicID.Desert;
		Main.curMusic = MusicID.Desert;
		Main.LocalPlayer.ZoneBeach = false;
		Main.LocalPlayer.ZoneSandstorm = Sandstorm.Happening;
	}
}
