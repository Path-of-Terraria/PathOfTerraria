using PathOfTerraria.Content.Projectiles;
using PathOfTerraria.Common.Systems;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using PathOfTerraria.Common.Systems.DisableBuilding;
using Terraria.Enums;
using Terraria.Localization;
using PathOfTerraria.Common.World.Generation;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Subworlds.BossDomains;

public class QueenBeeDomain : BossDomainSubworld
{
	public override int Width => 600;
	public override int Height => 600;
	public override int[] WhitelistedCutTiles => [TileID.BeeHive];

	public bool BossSpawned = false;
	public bool ReadyToExit = false;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep), new PassLegacy("Tiles", GenTiles)];

	private void GenTiles(GenerationProgress progress, GameConfiguration configuration)
	{
		Main.spawnTileX = Width / 2;
		Main.spawnTileY = Height / 2 - 20;
		Main.worldSurface = 230;
		Main.rockLayer = 299;

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");

		for (int x = 0; x < Main.maxTilesX; ++x)
		{
			for (int y = 0; y < Main.maxTilesY; ++y)
			{
				Tile tile = Main.tile[x, y];
				tile.TileType = TileID.Mud;
				tile.HasTile = true;
			}

			progress.Value = (float)x / Main.maxTilesX;
		}

		StructureTools.PlaceByOrigin("Assets/Structures/BeeDomain/Arena_" + WorldGen.genRand.Next(2), new Point16(Width / 2, Height / 2), new(0.5f));
	}

	public override void OnEnter()
	{
		BossSpawned = false;
		ReadyToExit = false;
	}

	public override void Update()
	{
		Main.dayTime = true;
		Main.time = Main.dayLength / 2;
		Main.moonPhase = (int)MoonPhase.Full;

		foreach (Player player in Main.ActivePlayers)
		{
			player.GetModPlayer<StopBuildingPlayer>().ConstantStopBuilding = true;
		}

		if (!BossSpawned && NPC.AnyNPCs(NPCID.QueenBee))
		{
			BossSpawned = true;
		}

		if (BossSpawned && !NPC.AnyNPCs(NPCID.QueenBee) && !ReadyToExit)
		{
			Vector2 pos = new Vector2(Width / 2, Height / 2 - 8) * 16;
			Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), pos, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);

			BossTracker.CachedBossesDowned.Add(NPCID.QueenBee);
			ReadyToExit = true;
		}
	}
}