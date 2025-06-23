using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain.Generation;
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
	public const int TerrariaHeight = 1800;
	public const int CloudTop = TerrariaHeight - 350;
	public const int CloudBottom = TerrariaHeight - 50;
	public const int PlanetTop = 700;

	public override int Width => 900;
	public override int Height => 4200;
	public override (int time, bool isDay) ForceTime => (3500, false);
	
	private static bool BossSpawned = false;
	private static bool ExitSpawned = false;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Terrain", MoonlordTerrainGen.GenerateTerraria),
		new PassLegacy("Clouds", GenerateClouds),
		new PassLegacy("Planets", GeneratePlanets)];

	private void GeneratePlanets(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Space");

		PriorityQueue<int, float> planetTypes = new();
		planetTypes.Enqueue(0, WorldGen.genRand.NextFloat());
		planetTypes.Enqueue(1, WorldGen.genRand.NextFloat());
		planetTypes.Enqueue(2, WorldGen.genRand.NextFloat());
		planetTypes.Enqueue(3, WorldGen.genRand.NextFloat());

		for (int i = 0; i < 4; ++i)
		{
			MoonlordPlanetGen.GeneratePlanet(i, planetTypes.Dequeue());
		}
	}

	private void GenerateClouds(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Clouds");
		MoonlordCloudGen.GenerateClouds(progress, Width, Height);
	}

	public override void OnEnter()
	{
		base.OnEnter();

		BossSpawned = false;
		ExitSpawned = false;
	}

	public override void Update()
	{
		Main.shimmerAlpha = 1f;

		if (!BossSpawned && NPC.AnyNPCs(NPCID.MoonLordCore))
		{
			BossSpawned = true;
		}

		ModifySpawn();

		if (BossSpawned && !NPC.AnyNPCs(NPCID.MoonLordCore) && !ExitSpawned)
		{
			ExitSpawned = true;

			HashSet<Player> players = [];

			foreach (Player plr in Main.ActivePlayers)
			{
				if (!plr.dead)
				{
					players.Add(plr);
				}
			}

			IEntitySource src = Entity.GetSource_NaturalSpawn();
			Vector2 position = Main.rand.Next([.. players]).Center - new Vector2(0, 60);
			Projectile.NewProjectile(src, position, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);
		}
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
	}

	private static void ModifySpawnCutoff(int highestX, int highestY, float cutoffFactor)
	{
		int y = (int)MathHelper.Lerp(TerrariaHeight, Main.maxTilesY, cutoffFactor);

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
