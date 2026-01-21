using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PathOfTerraria.Common.Systems.Synchronization;
using PathOfTerraria.Content.Conflux;
using PathOfTerraria.Utilities.Terraria;
using PathOfTerraria.Utilities.Xna;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.Config;

namespace PathOfTerraria.Common.Encounters;

internal enum EnemySpawnEffect
{
	None,
	Teleport,
	GlacialRift,
	InfernalRift,
	CelestialRift
}

/// <summary> An enemy spawn description. </summary>
internal record struct EnemySpawn()
{
	/// <summary> The NPC type to spawn. </summary>
	[JsonConverter(typeof(EntityDefinitionJsonConverter))]
	public required NPCDefinition NpcType { get; set; }
	/// <summary> Pre-determined spawn position to use. Mutually exclusive with <see cref="SpawnPlacement"/>. </summary>
	public required Vector2? SpawnPosition { get; set; }
	/// <summary> Pre-determined spawn position to use. Mutually exclusive with <see cref="SpawnPlacement"/>. </summary>
	public required SpawnPlacement? SpawnPlacement { get; set; }
	/// <summary> Which effect to use for this spawn. </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public EnemySpawnEffect Effect { get; set; }

	/// <summary> The time in ticks that must pass before the next enemy in queue will be spawned. </summary>
	[Range(0, 5 * 60 * 60), DefaultValue(0)]
	public uint CooldownInTicks { get; set; } = 0;
	/// <summary> The encounter kill score given for killing this enemy. </summary>
	[DefaultValue(1f)]
	public float KillScore { get; set; } = 1f;
}

/// <summary> A description used to calculate a placement for an enemy spawn. </summary>
internal record struct SpawnPlacement()
{
	/// <summary> The required free space's width and height, in pixels. </summary>
	public required Point CollisionSize { get; set; }
	/// <summary> The available spawn area to use, in tile-space. </summary>
	public required Rectangle Area { get; set; }
	/// <summary> If provided, a pathfinding check will be performed towards this tile-space position to determine if a given spawn position is valid. </summary>
	public Point16? AreaOrigin { get; set; }
	/// <summary> Whether this enemy has to spawn on the ground. </summary>
	public bool OnGround { get; set; }
	/// <summary> If not zero, the spawn point must not intersect with any of the liquids within this mask. </summary>
	[DefaultValue(LiquidMask.All)]
	public LiquidMask SkippedLiquids { get; set; } = LiquidMask.All;
	/// <summary> If not zero, the spawn point must be submerged into any of the liquids within this mask. </summary>
	[DefaultValue(LiquidMask.None)]
	public LiquidMask RequiredLiquids { get; set; } = LiquidMask.None;
	/// <summary> How far away from players, in pixels, must the spawn be placed. </summary>
	[Range(0, 2048), Increment(8), DefaultValue(256f)]
	public float MinDistanceFromPlayers { get; set; } = 256f;
	/// <summary> How far away from existing enemies, in pixels, must the spawn be placed. </summary>
	[Range(0, 2048), Increment(8), DefaultValue(256f)]
	public float MinDistanceFromEnemies { get; set; } = 256f;
	/// <summary> How many dynamic placement attempts to perform before giving up. High values may affect performance. </summary>
	[Range(1, 1024), Increment(1), DefaultValue(10)]
	public int MaxSearchAttempts { get; set; } = 10;

	/// <summary> Guesses defaults to use for a given NPC sample. Not always ideal. </summary>
	public SpawnPlacement WithDefaults(int npcType)
	{
		NPC npc = ContentSamples.NpcsByNetId[npcType];
		bool isFlying = npc.aiStyle is NPCAIStyleID.Flying or NPCAIStyleID.FlyingFish;
		bool isSwimming = npc.aiStyle is NPCAIStyleID.Piranha;

		OnGround = !isFlying && !isSwimming;
		RequiredLiquids = isSwimming ? LiquidMask.Water : LiquidMask.None;
		SkippedLiquids = isSwimming ? LiquidMask.None : LiquidMask.All;

		return this;
	}
}

/// <summary> Functions for spawning enemies in automated and fancy ways. </summary>
internal static class EnemySpawning
{
	/// <summary>
	/// Synchronizes <see cref="EnemySpawning"/>'s enemy spawn effects. Should be called on servers only.
	/// </summary>
	private sealed class EnemySpawnHandler : Handler
	{
		public static void Send(NPC npc, EnemySpawnEffect effect, Vector2 position)
		{
			ModPacket packet = Networking.GetPacket<EnemySpawnHandler>();
			packet.Write((byte)npc.whoAmI);
			packet.Write((byte)effect);
			packet.WriteVector2(position);
			packet.Send();
		}

		internal override void ServerReceive(BinaryReader reader, byte sender)
		{
		}

		internal override void ClientReceive(BinaryReader reader, byte sender)
		{
			byte npcIndex = reader.ReadByte();
			var effect = (EnemySpawnEffect)reader.ReadByte();
			Vector2 position = reader.ReadVector2();

			if (npcIndex < Main.maxNPCs && Main.npc[npcIndex] is { active: true } npc && Main.netMode == NetmodeID.MultiplayerClient)
			{
				SpawnEffects(npc, effect, position);
			}
		}
	}

	/// <summary>
	/// Attempts to spawn an enemy in the given area, with the given parameters.
	/// <br/> Can fail if spawn logic gives up on picking a viable position.
	/// <br/> Will fail if there are too many enemies in the world.
	/// </summary>
	public static bool TrySpawningEnemy(IEntitySource source, in EnemySpawn spawn, [NotNullWhen(true)] out NPC npc)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
		{
			throw new InvalidOperationException("Attempted to spawn NPC on multiplayer client.");
		}

		if (spawn.SpawnPosition.HasValue == spawn.SpawnPlacement.HasValue)
		{
			throw new InvalidOperationException($"A spawn description must have either {spawn.SpawnPlacement} or {spawn.SpawnPosition} specified, not both nor neither.");
		}

		if (spawn.SpawnPosition is not { } spawnPosition)
		{
			if (!TryFindingSpawnPosition(out spawnPosition, spawn.SpawnPlacement!.Value))
			{
				npc = default;
				return false;
			}
		}

		npc = NPC.NewNPCDirect(source, spawnPosition, spawn.NpcType.Type);

		if (npc.whoAmI == Main.maxNPCs)
		{
			return false;
		}

		SpawnEffects(npc, spawn.Effect, spawnPosition);

		return true;
	}

	/// <summary>
	/// Triggers spawn effects for the given enemy at a given position. Synchronized from server to clients.
	/// </summary>
	public static void SpawnEffects(NPC npc, EnemySpawnEffect effect, Vector2 position)
	{
		if (Main.netMode == NetmodeID.Server)
		{
			EnemySpawnHandler.Send(npc, effect, position);
			return;
		}

		if (effect == EnemySpawnEffect.Teleport)
		{
			SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.25f, Pitch = -0.6f, PitchVariance = 0.2f }, position);

			for (int i = 0; i < 10; i++)
			{
				Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.WitherLightning);
			}
		}
		if (effect is EnemySpawnEffect.GlacialRift or EnemySpawnEffect.InfernalRift or EnemySpawnEffect.CelestialRift)
		{
			(Projectile? rift, float closestSqrDst) = (null, float.PositiveInfinity);

			foreach (Projectile projectile in Main.ActiveProjectiles)
			{
				if (projectile.ModProjectile is ConfluxRift projRift && projRift.Activated)
				{
					if (projectile.DistanceSQ(position) is float sqrDst && sqrDst > closestSqrDst) { continue; }
					(rift, closestSqrDst) = (projRift.Projectile, sqrDst);
				};
			}

			SoundEngine.PlaySound(position: position, style: new($"{nameof(PathOfTerraria)}/Assets/Sounds/Conflux/RiftEnemySpawn")
			{
				Volume = 0.37f,
				MaxInstances = 3,
				PitchVariance = 0.2f,
			});

			if (rift != null)
			{
				int dustID = 173;
				float dustVelocity = 0;
				float dustDensity = 1;
				float dustScale = 1;

				switch (effect)
				{
					case EnemySpawnEffect.CelestialRift:
						dustID = 173;
						dustVelocity = 0.2f;
						dustDensity = 32;
						dustScale = 1.5f;

						for (int i = 0; i < 10; i++)
						{
							Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.WitherLightning);
						}
						break;
					case EnemySpawnEffect.GlacialRift:
						dustID = 185;
						dustVelocity = 0.3f;
						dustDensity = 32;
						dustScale = 1.5f;

						for (int i = 0; i < 10; i++)
						{
							Dust.NewDustDirect(npc.position, npc.width, npc.height, 226);
						}
						break;
					case EnemySpawnEffect.InfernalRift:
						dustID = 127;
						dustVelocity = 0.5f;
						dustDensity = 32;
						dustScale = 1.65f;

						for (int i = 0; i < 20; i++)
						{
							Dust.NewDustDirect(npc.position, npc.width, npc.height, 174);
						}
						break;
				}

				Vector2 lineStart = rift.Center + Main.rand.NextVector2Circular(32f, 32f);

				for (float i = 0, step = dustDensity * dustDensity; i < closestSqrDst; i += step)
				{
					var dustPos = Vector2.Lerp(lineStart, position, i / closestSqrDst);
					dustPos += Main.rand.NextVector2Circular(5f, 5f);

					Dust.NewDustPerfect(dustPos, dustID, Vector2.One.RotatedByRandom(6.28) * dustVelocity, 0, default, dustScale).noGravity = true;
				}
			}
		}
	}

	/// <summary>
	/// Attempts to pick a suitable spawn position with the given <see cref="SpawnPlacement"/> options.
	/// </summary>
	public static bool TryFindingSpawnPosition(out Vector2 spawnPosition, in SpawnPlacement spawn)
	{
		if (spawn.Area == default)
		{
			throw new ArgumentException("Invalid area.");
		}

		float minSqrDistanceFromPlayers = spawn.MinDistanceFromPlayers * spawn.MinDistanceFromPlayers;
		float minSqrDistanceFromEnemies = spawn.MinDistanceFromEnemies * spawn.MinDistanceFromEnemies;
		bool checksPlayerAdjacency = minSqrDistanceFromPlayers > 0f;
		bool checksEnemyAdjacency = minSqrDistanceFromEnemies > 0f;
		bool checksForLiquids = (spawn.SkippedLiquids | spawn.RequiredLiquids) != 0;

		var sizeInTiles = new Vector2Int(
			(int)MathF.Ceiling(spawn.CollisionSize.X / (float)TileUtils.TileSizeInPixels),
			(int)MathF.Ceiling(spawn.CollisionSize.Y / (float)TileUtils.TileSizeInPixels)
		);
		Vector2 sizeInTilesHalf = sizeInTiles * 0.5f;
		var sizeTileExtents = new Vector4Int
		(
			(int)-MathF.Floor(-sizeInTilesHalf.X),
			(int)-MathF.Floor(-sizeInTilesHalf.Y),
			(int)+MathF.Floor(+sizeInTilesHalf.X) - 1,
			(int)+MathF.Floor(+sizeInTilesHalf.Y) - 1
		);

		// Exclusive upper bounds.
		var area = new Vector4Int
		(
			Math.Max(0, spawn.Area.X),
			Math.Max(0, spawn.Area.Y),
			Math.Min(Main.maxTilesX, spawn.Area.X + spawn.Area.Width),
			Math.Min(Main.maxTilesY, spawn.Area.Y + spawn.Area.Height)
		);

		// Ensure that the spawn origin is in a location reachable by inflated floodfill.
		Vector2Int adjustedSpawnOrigin = default;
		if (spawn.AreaOrigin is { } spawnOrigin && !TileUtils.TryFitRectangleIntoTilemap(spawnOrigin, sizeInTiles, out adjustedSpawnOrigin))
		{
#if DEBUG
			string message = "Attempted to find an enemy spawn with a bad origin point given!";
			PoTMod.Instance.Logger.Warn(message);
			Main.NewText($"Warning: {message}", Color.MediumVioletRed);
#endif
			spawnPosition = default;
			return false;
		}

		for (int i = 0; i < spawn.MaxSearchAttempts; i++)
		{
			(int x, int y) = (Main.rand.Next(area.X, area.Z), Main.rand.Next(area.Y, area.W));

			Tile baseTile = Main.tile[x, y];
			if (TileUtils.HasUnactuatedSolid(baseTile)) { continue; }

			// Move the spawn point to the ground if needed.
			if (spawn.OnGround)
			{
				for (int yy = y; yy < Main.maxTilesY + 1; yy++)
				{
					if (TileUtils.HasUnactuatedSolid(Main.tile[x, yy]))
					{
						y = yy - 1;
						break;
					}
				}
			}

			// Ensure that the ground position has enough space.
			// While doing this, we also offset the spawn position off the ground, so that the later
			// floodfill cycle does not think that we are in a wall due to collision check inflation.
			if (TileUtils.TryFitRectangleIntoTilemap(new Vector2Int(x, y), sizeInTiles, out Vector2Int adjustedPoint))
			{
				x = adjustedPoint.X;
				y = adjustedPoint.Y;
			}
			else
			{
				continue;
			}

			spawnPosition = new Point16(x, y).ToWorldCoordinates(autoAddY: spawn.OnGround ? 15 : 8);

			// Ensure that this is not too close to a player.
			if (checksPlayerAdjacency)
			{
				foreach (Player player in Main.ActivePlayers)
				{
					if (player.DistanceSQ(spawnPosition) <= minSqrDistanceFromPlayers)
					{
						goto Continue;
					}
				}
			}

			// Ensure that this is not too close to an existing enemy.
			if (checksEnemyAdjacency)
			{
				foreach (NPC npc in Main.ActiveNPCs)
				{
					if (npc.DistanceSQ(spawnPosition) <= minSqrDistanceFromEnemies)
					{
						goto Continue;
					}
				}
			}

			// Check for liquids if needed.
			if (checksForLiquids)
			{
				if (!LiquidUtils.CheckAreaWithMasks(new Rectangle(x, y, sizeInTiles.X, sizeInTiles.Y), spawn.SkippedLiquids, spawn.RequiredLiquids))
				{
					continue;
				}
			}

			// As the last check, perform an optional floodfill loop to see if we can reach the encounter's center.
			if (spawn.AreaOrigin.HasValue && !PathfindToSpawnOrigin(area, new(x, y), adjustedSpawnOrigin, sizeTileExtents))
			{
				continue;
			}

			// Success, all the checks have passed.
			return true;

			// Failure, some deep loop has given up.
			Continue:;
		}

		spawnPosition = default;
		return false;
	}

	private static bool PathfindToSpawnOrigin(Vector4Int baseArea, Vector2Int startPoint, Vector2Int target, Vector4Int sizeTileExtents)
	{
		const int FloodFillExtent = 8;

		// Inclusive upper bounds.
		var area = new Vector4Int(
			Math.Max(baseArea.X - FloodFillExtent, 1 + sizeTileExtents.X),
			Math.Max(baseArea.Y - FloodFillExtent, 1 + sizeTileExtents.Y),
			Math.Min(baseArea.Z + FloodFillExtent, Main.maxTilesX - 2 - sizeTileExtents.Z),
			Math.Min(baseArea.W + FloodFillExtent, Main.maxTilesY - 2 - sizeTileExtents.W)
		);
		var rect = new Rectangle(
			area.X,
			area.Y,
			area.Z - area.X,
			area.W - area.Y
		);

		startPoint.X = Math.Min(area.X, Math.Max(startPoint.X, area.Z)); 
		startPoint.Y = Math.Min(area.Y, Math.Max(startPoint.Y, area.W));

		foreach (GeometryUtils.FloodFill.Result step in new GeometryUtils.FloodFill(startPoint, rect))
		{
			// Inclusive.
			(int checkX1, int checkX2) = (step.Point.X - sizeTileExtents.X, step.Point.X + sizeTileExtents.Z);
			(int checkY1, int checkY2) = (step.Point.Y - sizeTileExtents.Y, step.Point.Y + sizeTileExtents.W);
			bool isPointFree = true;

			for (int checkX = checkX1; checkX <= checkX2; checkX++)
			{
				for (int checkY = checkY1; checkY <= checkY2; checkY++)
				{
					if (TileUtils.HasUnactuatedSolid(Main.tile[checkX, checkY]))
					{
						isPointFree = false;
						goto CycleBreak;
					}
				}
			}

			CycleBreak: step.IsPointFree = isPointFree;

			if (step.Point.X == target.X & step.Point.Y == target.Y)
			{
				return true;
			}
		}

		// Failure, the origin point has never been reached.
		return false;
	}
}
