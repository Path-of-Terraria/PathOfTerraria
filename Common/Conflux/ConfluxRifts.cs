using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Encounters;
using PathOfTerraria.Content.Conflux;
using PathOfTerraria.Utilities.Terraria;
using PathOfTerraria.Utilities.Xna;
using SubworldLibrary;
using Terraria.DataStructures;
using Terraria.ID;

#nullable enable

namespace PathOfTerraria.Common.Conflux;

internal sealed class ConfluxRifts : ModSystem
{
	private static bool checkedRifts;

	public override void ClearWorld()
	{
		checkedRifts = false;
	}

	public override void PostUpdateWorld()
	{
		if (!checkedRifts)
		{
			SpawnRifts();
			checkedRifts = true;
		}
	}

	/// <summary> Attempts to spawn conflux rifts, returns the amount created. </summary>
	public static int SpawnRifts()
	{
		if (SubworldSystem.Current is null) { return 0; }

		if (Main.netMode == NetmodeID.MultiplayerClient) { return 0; }

		const int worldEdgeOffset = 40;
		const int freeSpaceInTiles = 10;
		const float minDistanceFromRifts = 2048f;
		const float minDistanceFromRiftsSqr = minDistanceFromRifts * minDistanceFromRifts;

		var placement = new SpawnPlacement
		{
			Area = new Rectangle(worldEdgeOffset, worldEdgeOffset, Main.maxTilesX - (worldEdgeOffset * 2), Main.maxTilesY - (worldEdgeOffset * 2)),
			CollisionSize = new(freeSpaceInTiles * TileUtils.TileSizeInPixels, freeSpaceInTiles * TileUtils.TileSizeInPixels),
			OnGround = true,
			MinDistanceFromEnemies = 0f,
			MinDistanceFromPlayers = 2048f,
			MaxSearchAttempts = 4096,
		};

		int targetRifts = 3;
		IEntitySource? source = Entity.GetSource_None();
		var rifts = new List<Projectile>(capacity: targetRifts);

		for (int i = 0; i < targetRifts; i++)
		{
			if (!EnemySpawning.TryFindingSpawnPosition(in placement, out Vector2 position))
			{
				break;
			}

			if (rifts.Any(p => p.DistanceSQ(position) < minDistanceFromRiftsSqr))
			{
				continue;
			}

			position.Y += 5 * TileUtils.PixelSizeInUnits;

			var kind = (ConfluxRiftKind)(i % ((int)ConfluxRiftKind.Count));
			var rift = Projectile.NewProjectileDirect(source, position, Vector2.Zero, ModContent.ProjectileType<ConfluxRift>(), 0, 0f, ai0: (float)kind);

			rifts.Add(rift);
		}

		return rifts.Count;
	}

	public static void ActivateRift(Projectile rift)
	{
		const int extentsW = 40;
		const int extentsH = 20;
		Vector2 worldOrigin = rift.Center;
		Point16 tileOrigin = worldOrigin.ToTileCoordinates16();

		var area = new Vector4Int
		{
			X = Math.Max(tileOrigin.X - extentsW, 0),
			Y = Math.Max(tileOrigin.Y - extentsH, 0),
			Z = Math.Min(tileOrigin.X + extentsW, Main.maxTilesX - 1),
			W = Math.Min(tileOrigin.Y + extentsH, Main.maxTilesY - 1),
		};
		var rectangle = new Rectangle(area.X, area.Y, area.Z - area.X, area.W - area.Y);

		int[] possibleTypes =
		[
			NPCID.Zombie,
			NPCID.ArmedZombie,
			NPCID.Skeleton,
			NPCID.EaterofSouls,
			NPCID.BigEater,
			NPCID.DemonEye,
		];

		var waves = new EncounterWave[15];

		for (int waveIndex = 0; waveIndex < waves.Length; waveIndex++)
		{
			var spawns = new EnemySpawn[Main.rand.Next(10, 15)];

			for (int spawnIndex = 0; spawnIndex < spawns.Length; spawnIndex++)
			{
				int type = possibleTypes[Main.rand.Next(possibleTypes.Length)];
				spawns[spawnIndex] = new EnemySpawn
				{
					NpcType = new(type),
					SpawnPosition = null,
					SpawnPlacement = new SpawnPlacement
					{
						Area = default,
						CollisionSize = default,
						OnGround = false,
					},
					Effect = EnemySpawnEffect.Teleport,
					CooldownInTicks = spawnIndex < 5 ? 0 : (uint)Main.rand.Next(30, 45),
				};
			}

			waves[waveIndex] = new EncounterWave
			{
				Spawns = spawns,
			};
		}

		EnemyEncounters.CreateEncounter(new EncounterDescription
		{
			Identifier = "Rift",
			SpawnOrigin = tileOrigin,
			ActivationOrigin = worldOrigin,
			ActivationRange = 2048f,
			SpawnArea = rectangle,
			Waves = waves,
			Music = new(MusicID.LunarBoss),
			SceneEffectPriority = SceneEffectPriority.BossLow,
		});
	}
}
