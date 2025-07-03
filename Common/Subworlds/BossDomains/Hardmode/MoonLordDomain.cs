using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain;
using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain.Generation;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.World.Generation.Tools;
using PathOfTerraria.Common.World.Passes;
using PathOfTerraria.Content.Projectiles.Utility;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;

internal class MoonLordDomain : BossDomainSubworld
{
	public const int TerrariaHeight = 2400;
	public const int CloudTop = TerrariaHeight - 350;
	public const int CloudBottom = TerrariaHeight - 50;
	public const int PlanetTop = 1200;
	public const int TopOfTheWorld = PlanetTop / 2;

	public override int Width => 1200;
	public override int Height => 4800;
	public override (int time, bool isDay) ForceTime => (3500, false);

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Terrain", MoonlordTerrainGen.GenerateTerraria),
		new PassLegacy("Clouds", GenerateClouds),
		new PassLegacy("Planets", GeneratePlanets),
		new PassLegacy("Clean", CleanWorld),
		new PassLegacy("Settle Liquids", SettleLiquidsStep.Generation),
		new PassLegacy("Shimmerize", ShimmerizeTopTerrain)];

	private void ShimmerizeTopTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		for (int i = 20; i < Width - 20; ++i)
		{
			for (int j = TerrariaHeight; j < TerrariaHeight + 300; ++j)
			{
				Tile tile = Main.tile[i, j];
				tile.LiquidType = LiquidID.Shimmer;
			}
		}

		for (int i = Main.spawnTileX - 520; i < Main.spawnTileX + 520; ++i)
		{
			for (int j = Main.spawnTileY - 160; j < Main.spawnTileY + 150; ++j)
			{
				if (MoonlordTerrainGen.ProtectedLiquid.Contains(new Point16(i, j)))
				{
					continue;
				}

				Tile tile = Main.tile[i, j];
				tile.LiquidAmount = 0;
			}
		}

		MoonlordBackgroundDrawing.ReloadPlanets();
	}

	private void CleanWorld(GenerationProgress progress, GameConfiguration configuration)
	{
		FastNoiseLite noise = MoonlordTerrainGen.GetTerrariaNoise();

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = TerrariaHeight + 50; j < Main.maxTilesY - 2; ++j)
			{
				Tile tile = Main.tile[i, j];
				OpenFlags flags = OpenExtensions.GetOpenings(i, j, false, false);

				if (tile.WallType == WallID.None)
				{
					tile.WallType = WallID.LunarRustBrickWall;
				}

				if (!tile.HasTile)
				{
					continue;
				}

				if (tile.TileType == TileID.Dirt && flags != OpenFlags.None)
				{
					tile.TileType = TileID.Grass;
					Decoration.OnPurityGrass(new Point16(i, j), flags, 1);
				}
				else if (tile.TileType == TileID.Stone && flags != OpenFlags.None)
				{
					tile.TileType = noise.GetNoise(i, j) switch
					{
						< -0.4f => TileID.ArgonMoss,
						< -0.2f => TileID.LavaMoss,
						< 0f => TileID.VioletMoss,
						< 0.2f => TileID.XenonMoss,
						< 0.4f => TileID.KryptonMoss,
						_ => TileID.Stone,
					};
				}
			}
		}
	}

	private void GeneratePlanets(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Space");

		PriorityQueue<MoonlordPlanetGen.PlanetType, float> planetTypes = new();
		planetTypes.Enqueue(MoonlordPlanetGen.PlanetType.Solar, WorldGen.genRand.NextFloat());
		planetTypes.Enqueue(MoonlordPlanetGen.PlanetType.Stardust, WorldGen.genRand.NextFloat());
		planetTypes.Enqueue(MoonlordPlanetGen.PlanetType.Nebula, WorldGen.genRand.NextFloat());
		planetTypes.Enqueue(MoonlordPlanetGen.PlanetType.Vortex, WorldGen.genRand.NextFloat());
		List<MoonlordPlanetGen.PlanetInstance> planets = [];

		for (int i = 0; i < 4; ++i)
		{
			MoonlordPlanetGen.PlanetType type = planetTypes.Dequeue();

			for (int j = 0; j < 4; ++j)
			{
				MoonlordPlanetGen.GeneratePlanet(type, planets);

				progress.Set((i * 4 + j) / 16f);
			}
		}
	}

	private void GenerateClouds(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Clouds");
		MoonlordCloudGen.GenerateClouds(progress, Width);
	}

	public override void OnEnter()
	{
		base.OnEnter();
	}

	public override void Update()
	{
		Liquid.UpdateLiquid();
		ModifySpawn();
	}

	private static void ModifySpawn()
	{
		int highestX = 0;
		int highestY = Main.maxTilesY * 16;

		foreach (Player plr in Main.ActivePlayers)
		{
			if (!plr.dead && !plr.ghost)
			{
				if (highestY > plr.Center.Y)
				{
					highestX = (int)plr.Center.X;
					highestY = (int)plr.Center.Y;
				}
			}
		}

		ModifySpawnCutoff(highestX, highestY, MoonlordTerrainGen.DirtCutoff);
		ModifySpawnCutoff(highestX, highestY, MoonlordTerrainGen.StoneCutoff);
		ModifySpawn(highestX, highestY, TerrariaHeight);
		ModifySpawn(highestX, highestY, CloudTop);
	}

	private static void ModifySpawnCutoff(int highestX, int highestY, float cutoffFactor)
	{
		ModifySpawn(highestX, highestY, (int)MathHelper.Lerp(TerrariaHeight, Main.maxTilesY, cutoffFactor));
	}

	private static void ModifySpawn(int highestX, int highestY, int y)
	{
		if (highestY < y * 16 && Main.spawnTileY > y)
		{
			Main.spawnTileX = highestX / 16;
			Main.spawnTileY = highestY / 16;

			if (Main.netMode == NetmodeID.Server)
			{
				NetMessage.SendData(MessageID.WorldData);
			}
		}
	}
}
