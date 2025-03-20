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

	public override int Width => 1800 + 120 * Main.rand.Next(4);
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
		const int MinHeight = 210;

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");

		Main.worldSurface = 240;
		Main.rockLayer = 270;

		LeftSpawn = Main.rand.NextBool(2);
		Main.spawnTileX = LeftSpawn ? 70 : Main.maxTilesX - 70;

		FastNoiseLite noise = new(WorldGen._genRandSeed);
		noise.SetFrequency(0.005f);
		noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);

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
					tile.TileType = TileID.Sand;
					tile.HasTile = true;

					if (i == Main.spawnTileX && j == (int)cutOffY + 1)
					{
						Main.spawnTileY = j - 4;
					}
				}
			}

			cutOffY -= (noise.GetNoise(i, 0) + 0.5f) * 1f;

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
}
