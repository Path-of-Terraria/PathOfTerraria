using System.Collections.Generic;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Projectiles.Utility;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Content.Conflux;

internal class InfernalRealm : BossDomainSubworld, IOverrideBiome
{
	private static Point16 arenaCenter;
	
	public FightTracker FightTracker = new([ModContent.NPCType<InfernalBoss>()])
	{
		ResetOnVanish = true,
		HaltTimeOnVanish = 60 * 10,
	};

	public override int Width => 768;
	public override int Height => 768;
	public override (int time, bool isDay) ForceTime => ((int)(Main.dayLength * 0.96), true);

	public override List<GenPass> Tasks =>
	[
		new PassLegacy("Reset", ResetStep),
		new PassLegacy("Terrain", GenTerrain)
	];

	public override void OnEnter()
	{
		base.OnEnter();
		FightTracker.Reset();
	}

	public override void Update()
	{
		FightState state = FightTracker.UpdateState();

		if (state == FightState.NotStarted)
		{
			// Spawn the boss.
			Point16 spawnPos = arenaCenter.ToWorldCoordinates().ToPoint16();
			NPC.NewNPC(Entity.GetSource_NaturalSpawn(), spawnPos.X, spawnPos.Y, ModContent.NPCType<InfernalBoss>());
		}
		else if (state == FightState.JustCompleted)
		{
			IEntitySource src = Entity.GetSource_NaturalSpawn();
			Vector2 spawnPos = new Point16(Main.maxTilesX, Main.maxTilesY).ToWorldCoordinates() * 0.5f;
			Projectile.NewProjectile(src, spawnPos, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0);
		}
	}

	void IOverrideBiome.OverrideBiome()
	{
		Main.LocalPlayer.ZoneSkyHeight = true;
		Main.LocalPlayer.ZoneMeteor = true;
		Main.newMusic = MusicID.Eclipse;
		Main.curMusic = MusicID.Eclipse;
		Main.windSpeedCurrent = 0;
		Main.windSpeedTarget = 0;
		Main.numClouds = 0;
		Main.cloudBGActive = 0f;
		Main.cloudAlpha = 0f;
		Main.cloudBGAlpha = 0f;
		Main.bgStyle = 15;
		Main.bloodMoon = true;
	}

	private void GenTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		int lavaLevel = (int)(Height / 3 * 2);

		arenaCenter = new(Main.maxTilesX / 2, Main.maxTilesY / 2);
		Main.spawnTileX = arenaCenter.X - 90;
		Main.spawnTileY = arenaCenter.Y - 10;
		Main.worldSurface = lavaLevel;
		Main.rockLayer = lavaLevel - 4;

		const string Arena = "Assets/Structures/Infernal/Arena";
		Point16 structSize = StructureTools.GetSize(Arena);
		Point16 structPos = StructureTools.PlaceByOrigin(Arena, arenaCenter, new Vector2(0.5f, 0.4f));

		for (int x = 0; x < Main.maxTilesX; x++)
		{
			for (int y = 0; y < Main.maxTilesY; y++)
			{
				Tile tile = Main.tile[x, y];

				if (y >= lavaLevel)
				{
					(tile.LiquidType, tile.LiquidAmount) = (LiquidID.Lava, byte.MaxValue);
				}
			}
		}
	}
}
