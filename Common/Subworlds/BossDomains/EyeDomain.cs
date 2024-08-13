using PathOfTerraria.Content.Projectiles;
using PathOfTerraria.Content.Tiles.BossDomain;
using PathOfTerraria.Common.Subworlds.Passes;
using PathOfTerraria.Common.Systems;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Utilities;
using Terraria.WorldBuilding;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.Systems.DisableBuilding;
using Humanizer;

namespace PathOfTerraria.Common.Subworlds.BossDomains;

public class EyeDomain : BossDomainSubworld
{
	public const int ArenaX = 620;

	public override int Width => 800;
	public override int Height => 400;

	internal static Point16 ArenaEntrance = Point16.Zero;

	public Rectangle Arena = Rectangle.Empty;
	public bool BossSpawned = false;
	public bool ReadyToExit = false;
	public List<Vector2> SlimePositions = [];

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Surface", GenSurface),
		new PassLegacy("Grass", PlaceGrassAndDecor)];

	private void PlaceGrassAndDecor(GenerationProgress progress, GameConfiguration configuration)
	{
		Dictionary<Point16, Open> tiles = [];

		for (int i = 0; i < Main.maxTilesX; ++i)
		{
			for (int j = 80; j < Main.maxTilesY - 60; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (!tile.HasTile || tile.TileType != TileID.Dirt || tiles.ContainsKey(new Point16(i, j)))
				{
					continue;
				}

				Open flags = Open.None;

				if (!Main.tile[i, j - 1].HasTile)
				{
					flags |= Open.Above;
				}

				if (!Main.tile[i, j + 1].HasTile)
				{
					flags |= Open.Below;
				}

				if (flags == Open.None)
				{
					continue;
				}

				tiles.Add(new Point16(i, j), flags);
			}
		}

		int arenaY = 0;
		HashSet<Point16> grasses = [];

		foreach ((Point16 position, Open tile) in tiles)
		{
			TrySpreadGrassOnTile(tile, position, grasses);

			if (position.X == ArenaX && Main.tile[position].HasTile && Main.tile[position].TileType == TileID.Grass)
			{
				arenaY = position.Y;
			}
		}

		foreach (Point16 position in grasses)
		{
			if (!WorldGen.genRand.NextBool(3))
			{
				WorldGen.PlaceTile(position.X, position.Y - 1, TileID.Plants);
			}
			else if (WorldGen.genRand.NextBool(6) && position.X is > 20 and < 760)
			{
				WorldGen.PlaceTile(position.X, position.Y - 1, TileID.Saplings);
				
				if (!WorldGen.GrowTree(position.X, position.Y - 1))
				{
					WorldGen.KillTile(position.X, position.Y - 1);
				}
			}
		}

		StructureHelper.Generator.GenerateStructure("Assets/Structures/EyeArena", new Point16(ArenaX, arenaY - 27), Mod);
	}

	private static void TrySpreadGrassOnTile(Open adjacencies, Point16 position, HashSet<Point16> grasses)
	{
		Tile tile = Main.tile[position];

		if (adjacencies == Open.Above)
		{
			tile.TileType = TileID.Grass;

			if (!StepX(position.X) && position.X > 150 && WorldGen.genRand.NextBool(15))
			{
				WorldGen.TileRunner(position.X, position.Y, WorldGen.genRand.NextFloat(12, 26), WorldGen.genRand.Next(12, 40), TileID.FleshBlock);
			}
			else
			{
				grasses.Add(position);
			}
		}
	}

	private void GenSurface(GenerationProgress progress, GameConfiguration configuration)
	{
		Main.spawnTileX = 80;
		Main.spawnTileY = 210;
		Main.worldSurface = 230;
		Main.rockLayer = 299;

		float baseY = 220;

		FastNoiseLite noise = GetGenNoise();

		for (int x = 0; x < Main.maxTilesX; ++x)
		{
			if (StepX(x))
			{
				baseY -= WorldGen.genRand.NextFloat(0.8f);
			}

			float useY = baseY + noise.GetNoise(x, 0) * 4;

			for (int y = (int)useY; y < Main.maxTilesY; ++y)
			{
				WorldGen.PlaceTile(x, y, TileID.Dirt);
			}
		}
	}

	private static bool StepX(int x)
	{
		return x is > 100 and < 150 || x is > 300 and < 400 || x is > 500 and < 550;
	}

	private static FastNoiseLite GetGenNoise()
	{
		var noise = new FastNoiseLite();
		noise.SetFrequency(0.01f);
		return noise;
	}

	private void ResetStep(GenerationProgress progress, GameConfiguration configuration)
	{
		WorldGen._lastSeed = DateTime.Now.Second;
		WorldGen._genRand = new UnifiedRandom(DateTime.Now.Second);
		WorldGen._genRand.SetSeed(DateTime.Now.Second);
	}

	public override void OnEnter()
	{
		BossSpawned = false;
		ReadyToExit = false;
		SlimePositions.Clear();
	}

	public override void Update()
	{
		bool allInArena = true;

		foreach (Player player in Main.ActivePlayers)
		{
			player.GetModPlayer<StopBuildingPlayer>().ConstantStopBuilding = true;

			if (allInArena && !Arena.Intersects(player.Hitbox))
			{
				allInArena = false;
			}
		}

		if (!BossSpawned && allInArena)
		{
			for (int i = -6; i < 11; ++i)
			{
				WorldGen.PlaceTile(ArenaEntrance.X + i, ArenaEntrance.Y, TileID.SlimeBlock, true, true);
			}

			NPC.NewNPC(Entity.GetSource_NaturalSpawn(), Arena.Center.X, Arena.Center.Y + 400, NPCID.KingSlime);
			BossSpawned = true;
		}

		if (BossSpawned && !NPC.AnyNPCs(NPCID.KingSlime) && !ReadyToExit)
		{
			Vector2 pos = Arena.Center() + new Vector2(0, 150);
			Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), pos, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);

			BossTracker.CachedBossesDowned.Add(NPCID.KingSlime);
			ReadyToExit = true;
		}
	}
}