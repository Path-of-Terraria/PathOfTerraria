using PathOfTerraria.Common.ItemDropping;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.World.Passes;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Content.Tiles.BossDomain;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;

internal class QueenSlimeDomain : BossDomainSubworld
{
	public override int Width => 1200;
	public override int Height => 600;
	public override (int time, bool isDay) ForceTime => (3500, true);
	public override int[] WhitelistedMiningTiles => [ModContent.TileType<BrittleCrystal>()];

	internal static Point16 CircleCenter = Point16.Zero;

	private static bool LeftSpawn = false;
	private static Point16 ArenaPos = Point16.Zero;
	private static bool BossSpawned = false;
	private static bool PortalSpawned = false;
	private static bool ExitSpawned = false;
	private static Point16 PortalPos = Point16.Zero;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Base Terrain", SpawnBaseTerrain),
		new PassLegacy("Structures", SpawnStructures),
		new PassLegacy("Decor", SpawnDecor)];

	public override void OnEnter()
	{
		base.OnEnter();

		BossSpawned = false;
		PortalSpawned = false;
		ExitSpawned = false;
	}

	private void SpawnStructures(GenerationProgress progress, GameConfiguration configuration)
	{
		int x = Main.maxTilesX / 2 + (LeftSpawn ? 70 : -70);
		int y = 160;

		ArenaPos = new Point16(x, y);
		StructureTools.PlaceByOrigin("Assets/Structures/QueenSlimeDomain/Arena_0", ArenaPos, new Vector2(0.5f, 0.3f), noSync: true);
		ArenaPos = new Point16(ArenaPos.X - 4, ArenaPos.Y + 30);
	}

	private void SpawnDecor(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.PopulatingWorld");

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
					if (WorldGen.genRand.NextBool(2, 3))
					{
						Tile.SmoothSlope(i, j, false, false);
						tile = Main.tile[i, j];
					}

					if (tile.TileType == TileID.Dirt && tile.HasTile)
					{
						tile.TileType = TileID.HallowedGrass;

						grasses.Add(new Point16(i, j), flags);
					}
				}
			}

			progress.Value = (float)i / Main.maxTilesX;
		}

		foreach (KeyValuePair<Point16, OpenFlags> item in grasses)
		{
			DecorateGrass(item.Key, item.Value);
		}

		WeightedRandom<(int type, Range stackRange)> miscChestLoot = new();
		miscChestLoot.Add((ItemID.CrystalShard, 6..12), 1.4f);
		miscChestLoot.Add((ItemID.CrystalBullet, 20..30), 0.5f);
		miscChestLoot.Add((ItemID.SoulofLight, 4..8), 0.3f);
		miscChestLoot.Add((ItemID.PixieDust, 4..8), 0.8f);
		miscChestLoot.Add((ItemID.UnicornHorn, 1..1), 0.3f);
		miscChestLoot.Add((ItemID.BlessedApple, 1..1), 0.01f);
		miscChestLoot.Add((ItemID.Megaphone, 1..1), 0.01f);
		miscChestLoot.Add((ItemID.FastClock, 1..1), 0.01f);
		miscChestLoot.Add((ItemID.CrystalSerpent, 1..1), 0.01f);
		miscChestLoot.Add((ItemID.FairyCritterBlue, 1..1), 0.02f);
		miscChestLoot.Add((ItemID.FairyCritterGreen, 1..1), 0.02f);
		miscChestLoot.Add((ItemID.FairyCritterPink, 1..1), 0.02f);
		miscChestLoot.Add((ItemID.Prismite, 1..2), 0.15f);
		miscChestLoot.Add((ItemID.PrincessFish, 1..2), 0.2f);
		miscChestLoot.Add((ItemID.Dragonfruit, 1..3), 0.1f);
		miscChestLoot.Add((ItemID.Starfruit, 1..3), 0.1f);

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
				for (int k = 0; k < 8; ++k)
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

	private static void DecorateGrass(Point16 position, OpenFlags flags)
	{
		if (flags != OpenFlags.None)
		{
			if (WorldGen.genRand.NextBool(40) && WorldGen.SolidOrSlopedTile(position.X, position.Y))
			{
				string str = "Assets/Structures/QueenSlimeDomain/Crystal_" + Main.rand.Next(16);

				StructureTools.PlaceByOrigin(str, position, new Vector2(0.5f), noSync: true);
				return;
			}
		}

		if (!Main.tile[position].HasTile || Main.tile[position].TileType != TileID.HallowedGrass)
		{
			return;
		}

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
				int id = WorldGen.genRand.Next(15);
				int type = id <= 4 ? TileID.HallowedPlants2 : TileID.HallowedPlants;

				if (id == 14)
				{
					type = TileID.SmallPiles;
				}

				int styleRange = id == 0 ? 8 : 23;

				WorldGen.PlaceTile(position.X, position.Y - 1, type, style: WorldGen.genRand.Next(styleRange));
			}
		}
		
		if (flags.HasFlag(OpenFlags.Below) && WorldGen.genRand.NextBool(3, 5))
		{
			int count = WorldGen.genRand.Next(4, 9) + 1;

			for (int i = 1; i < count; ++i)
			{
				Tile tile = Main.tile[position.X, position.Y + i];
				tile.TileType = TileID.HallowedVines;
				tile.HasTile = true;
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
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");

		LeftSpawn = Main.rand.NextBool();
		Main.rockLayer = (int)(Height * 0.7f);
		Main.worldSurface = (int)(Height * 0.7f) - 50;

		CircleCenter = new(LeftSpawn ? 60 : Main.maxTilesX - 60, 60);
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
				float dist = ModDistance(new Vector2(i, j), CircleCenter.ToVector2());
				float cutoff = 650 + 10f * noise.GetNoise((new Vector2(i, j).AngleTo(CircleCenter.ToVector2()) + MathHelper.Pi) * 40, 0);
				float stoneCutoff = 780 + 20f * noise.GetNoise((new Vector2(i, j).AngleTo(CircleCenter.ToVector2()) + MathHelper.Pi) * 50, 0);

				bool stone = (stoneNoise.GetNoise(i, j) * (stoneNoise2.GetNoise(i, j) + 0.4f)) > Utils.GetLerpValue(stoneCutoff, cutoff, dist, true) * 0.15f;

				if (dist > cutoff)
				{
					Tile tile = Main.tile[i, j];
					tile.HasTile = true;
					tile.TileType = stone ? TileID.Pearlstone : TileID.Dirt;

					if (dist > cutoff + 4)
					{
						tile.WallType = stone ? WallID.PearlstoneEcho : WallID.DirtUnsafe;
					}
				}
			}

			progress.Value = (float)i / Main.maxTilesX;
		}

		DigTunnel();
	}

	private void DigTunnel()
	{
		FastNoiseLite noise = new(WorldGen._genRandSeed);
		HashSet<Point> crystals = [];

		DigSingleTunnel([new Vector2(LeftSpawn ? 200 : Main.maxTilesX - 200, 220),
			new Vector2(LeftSpawn ? 430 : Main.maxTilesX - 430, Height * 0.7f - 20), new Vector2(Width / 2, Height * 0.8f - 20)], noise, crystals, null);

		DigSingleTunnel([new Vector2(LeftSpawn ? 380 : Main.maxTilesX - 380, 200),
			new Vector2(LeftSpawn ? 550 : Main.maxTilesX - 550, Height * 0.6f - 20), new Vector2(Width / 2, Height * 0.8f - 20)], noise, crystals, null);

		int portalTunnel = WorldGen.genRand.Next(3);

		DigSingleTunnel([new Vector2(Width / 2, Height * 0.8f - 20),
			new Vector2(!LeftSpawn ? 400 : Main.maxTilesX - 400, Height * 0.45f),
			new Vector2(!LeftSpawn ? 300 : Main.maxTilesX - 300, Height * 0.25f - 20)], noise, crystals, portalTunnel == 0);

		DigSingleTunnel([new Vector2(Width / 2, Height * 0.8f - 20),
			new Vector2(!LeftSpawn ? 450 : Main.maxTilesX - 450, Height * 0.6f),
			new Vector2(!LeftSpawn ? 200 : Main.maxTilesX - 200, Height * 0.45f - 20)], noise, crystals, portalTunnel == 1);

		DigSingleTunnel([new Vector2(Width / 2, Height * 0.8f - 20),
			new Vector2(!LeftSpawn ? 500 : Main.maxTilesX - 500, Height * 0.725f),
			new Vector2(!LeftSpawn ? 100 : Main.maxTilesX - 100, Height * 0.65f - 20)], noise, crystals, portalTunnel == 2);

		foreach (Point crystal in crystals)
		{
			SpawnCrystalWall(crystal);
		}
	}

	private static void DigSingleTunnel(Vector2[] positions, FastNoiseLite noise, HashSet<Point> crystals, bool? spawnEnd)
	{
		positions = Tunnel.GeneratePoints(positions, 8, 6);
		Vector2 end = positions[^1];

		foreach (Vector2 pos in positions)
		{
			AddSpaceAndCrystals(noise, crystals, pos, end);
		}

		if (spawnEnd.HasValue)
		{
			if (spawnEnd.Value)
			{
				PortalPos = end.ToPoint16();
			}
			else
			{
				StructureTools.PlaceByOrigin("Assets/Structures/QueenSlimeDomain/Cove_" + Main.rand.Next(3), end.ToPoint16(), new Vector2(0.5f), noSync: true);
			}
		}
	}

	private static void AddSpaceAndCrystals(FastNoiseLite noise, HashSet<Point> crystals, Vector2 pos, Vector2 end)
	{
		TunnelDig(noise, pos);

		if (Main.rand.NextBool(20) && !crystals.Any(cry => Vector2.DistanceSquared(pos, cry.ToVector2()) < 60 * 60) && Vector2.DistanceSquared(end, pos) > 50 * 50)
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
			StructureTools.PlaceByOrigin(str, new Point16(pos.X, pair.Item1), new Vector2(0.5f), noSync: true);
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

	internal static float ModDistance(Vector2 position, Vector2 circleCenter)
	{
		return MathF.Sqrt(MathF.Pow(position.X - circleCenter.X, 2) + MathF.Pow(position.Y - circleCenter.Y, 2) * 5);
	}

	public override void Update()
	{
		if (!PortalSpawned)
		{
			int type = ModContent.ProjectileType<Teleportal>();
			Projectile.NewProjectile(new EntitySource_SpawnNPC(), PortalPos.ToWorldCoordinates(), Vector2.Zero, type, 0, 0, Main.myPlayer, ArenaPos.X * 16, ArenaPos.Y * 16);

			PortalSpawned = true;
		}

		if (!BossSpawned)
		{
			bool canSpawnBoss = Main.CurrentFrameFlags.ActivePlayersCount > 0;

			foreach (Player player in Main.ActivePlayers)
			{
				if (player.DistanceSQ(ArenaPos.ToWorldCoordinates()) > 1300 * 1300)
				{
					canSpawnBoss = false;
					break;
				}
			}

			if (canSpawnBoss)
			{
				BossSpawned = true;

				Main.spawnTileX = ArenaPos.X / 16;
				Main.spawnTileY = ArenaPos.Y / 16;

				NPC.NewNPC(Entity.GetSource_NaturalSpawn(), ArenaPos.X * 16, (ArenaPos.Y + 20) * 16, NPCID.QueenSlimeBoss);

				if (Main.netMode == NetmodeID.Server)
				{
					NetMessage.SendData(MessageID.WorldData);
				}
			}
		}
		else
		{
			if (!NPC.AnyNPCs(NPCID.QueenSlimeBoss) && !ExitSpawned)
			{
				ExitSpawned = true;

				IEntitySource src = Entity.GetSource_NaturalSpawn();
				Projectile.NewProjectile(src, ArenaPos.ToWorldCoordinates(), Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);
			}
		}
	}
}
