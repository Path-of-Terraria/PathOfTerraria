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

internal class DesertArea : MappingWorld
{
	public const int FloorY = 180;

	private static bool LeftSpawn = false;
	private static Point BossSpawnLocation = Point.Zero;

	public override int Width => 1200 + 120 * Main.rand.Next(10);
	public override int Height => 400;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep), new PassLegacy("Terrain", GenerateTerrain)];

	public override void OnEnter()
	{
		SubworldSystem.noReturn = true;
	}

	public override void Update()
	{
		bool hasPortal = false;

		foreach (Projectile projectile in Main.ActiveProjectiles)
		{
			if (projectile.type == ModContent.ProjectileType<ExitPortal>())
			{
				hasPortal = true;
				break;
			}
		}

		//if (!hasPortal && ModContent.GetInstance<GrovetenderSystem>().GrovetenderWhoAmI == -1 && !NPC.AnyNPCs(ModContent.NPCType<Grovetender>()))
		//{
		//	int npc = NPC.NewNPC(new EntitySource_SpawnNPC(), BossSpawnLocation.X, BossSpawnLocation.Y, ModContent.NPCType<Grovetender>());

		//	if (Main.netMode == NetmodeID.Server)
		//	{
		//		NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc);
		//	}
		//}
	}

	private void GenerateTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");

		Main.worldSurface = 240;
		Main.rockLayer = 270;

		LeftSpawn = Main.rand.NextBool(2);
		Main.spawnTileX = LeftSpawn ? 70 : Main.maxTilesX - 70;

		FastNoiseLite noise = new(WorldGen._genRandSeed);
		noise.SetFrequency(0.02f);
		noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);

		HashSet<Point16> leafBlobs = [];
		HashSet<int> trees = [];
		HashSet<int> duneXs = [];
		float cutOffY = 220;

		PopulateDuneLocations(duneXs);

		for (int i = 0; i < Main.maxTilesX; ++i)
		{
			progress.Set(i / (float)Main.maxTilesX);

			for (int j = 0; j < Main.maxTilesY; ++j)
			{
				Tile tile = Main.tile[i, j];
				
				if (j > cutOffY)
				{
					tile.TileType = TileID.Sand;
					tile.HasTile = true;
				}
			}

			if (duneXs.Contains(i))
			{
				cutOffY += noise.GetNoise(i, 0) * 1.2f;
			}
		}
	}

	private void PopulateDuneLocations(HashSet<int> duneXs)
	{
		List<Range> ranges = [];

		for (int i = 0; i < Width / 1200 * 7; ++i)
		{
			int baseX = WorldGen.genRand.Next(40, Main.maxTilesX - 500);
			ranges.Add(baseX..(baseX + WorldGen.genRand.Next(400)));
		}

		foreach (Range item in ranges)
		{
			for (int i = item.Start.Value; i < item.End.Value; ++i)
			{
				duneXs.Add(i);
			}
		}
	}
}
