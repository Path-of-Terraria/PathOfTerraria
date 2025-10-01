using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PathOfTerraria.Common.Systems.Synchronization;
using PathOfTerraria.Utilities.Terraria;
using PathOfTerraria.Utilities.Xna;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.Config;

namespace PathOfTerraria.Common.Encounters;

internal enum EnemySpawnEffect
{
	None,
	Teleport,
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
	/// <summary> The time in ticks that must pass before the next enemy in queue will be spawned. </summary>
	[Range(0, 5 * 60 * 60), DefaultValue(0)]
	public uint CooldownInTicks { get; set; } = 0;
	/// <summary> Which effect to use for this spawn. </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public EnemySpawnEffect Effect { get; set; }
}

/// <summary> A description used to calculate a placement for an enemy spawn. </summary>
internal record struct SpawnPlacement()
{
	/// <summary> The enemy's width and height. </summary>
	public required Point16 CollisionSize { get; set; }
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
	private sealed class EnemySpawnHandler : Handler
	{
		public override Networking.Message MessageType => Networking.Message.EnemySpawn;

		/// <inheritdoc cref="Networking.Message.EnemySpawn"/>
		public override void Send(params object[] parameters)
		{
			CastParameters(parameters, out NPC npc, out EnemySpawnEffect effect, out Vector2 position);

			ModPacket packet = Networking.GetPacket(MessageType);
			packet.Write((byte)npc.whoAmI);
			packet.Write((byte)effect);
			packet.WriteVector2(position);
			packet.Send();
		}

		internal override void ServerRecieve(BinaryReader reader) { }

		internal override void ClientRecieve(BinaryReader reader)
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
			if (!TryFindSpawnPosition(spawn.SpawnPlacement!.Value, out spawnPosition))
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

	private static void SpawnEffects(NPC npc, EnemySpawnEffect effect, Vector2 position)
	{
		if (Main.netMode == NetmodeID.Server)
		{
			ModContent.GetInstance<EnemySpawnHandler>().Send(npc, effect, position);
			return;
		}

		if (effect == EnemySpawnEffect.Teleport)
		{
			for (int i = 0; i < 10; i++)
			{
				Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.WitherLightning);
			}
		}
	}

	private static bool TryFindSpawnPosition(in SpawnPlacement spawn, out Vector2 spawnPosition)
	{
		if (spawn.Area == default)
		{
			throw new ArgumentException("Invalid area.");
		}

		const int MaxRandomAttempts = 10;

		float minSqrDistanceFromPlayers = spawn.MinDistanceFromPlayers * spawn.MinDistanceFromPlayers;
		float minSqrDistanceFromEnemies = spawn.MinDistanceFromEnemies * spawn.MinDistanceFromEnemies;

		var sizeInTiles = new Vector2Int(
			(int)MathF.Ceiling(spawn.CollisionSize.X / (float)TileUtils.TileSizeInPixels),
			(int)MathF.Ceiling(spawn.CollisionSize.Y / (float)TileUtils.TileSizeInPixels)
		);
		Vector2 sizeInTilesHalf = sizeInTiles * 0.5f;
		(int sizeOffsetX1, int sizeOffsetX2) = ((int)MathF.Floor(-sizeInTilesHalf.X), (int)MathF.Floor(sizeInTilesHalf.X) - 1);
		(int sizeOffsetY1, int sizeOffsetY2) = ((int)MathF.Floor(-sizeInTilesHalf.Y), (int)MathF.Floor(sizeInTilesHalf.Y) - 1);

		(int xMin, int xMax) = (Math.Max(0, spawn.Area.X), Math.Min(spawn.Area.X + spawn.Area.Width, Main.maxTilesX - 1));
		(int yMin, int yMax) = (Math.Max(0, spawn.Area.Y), Math.Min(spawn.Area.Y + spawn.Area.Height, Main.maxTilesY - 1));

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

		for (int i = 0; i < MaxRandomAttempts; i++)
		{
			(int x, int y) = (Main.rand.Next(xMin, xMax + 1), Main.rand.Next(yMin, yMax + 1));

			Tile baseTile = Main.tile[x, y];
			if (baseTile.HasUnactuatedTile && Main.tileSolid[baseTile.TileType]) { continue; }

			// Move the spawn point to the ground if needed.
			if (spawn.OnGround)
			{
				for (int yy = y; yy < Main.maxTilesY + 1; yy++)
				{
					Tile tile = Main.tile[x, yy];
					if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType])
					{
						y = yy - 1;
						break;
					}
				}
			}

			// Ensure that the ground position has enough space.
			// While doing this, we also offset the spawn position off the ground, so that the later
			// floodfill cycle does not think that we are in a wall due to collision check inflation.
			if (TileUtils.TryFitRectangleIntoTilemap(new Vector2Int(x, y), sizeInTiles, out Vector2Int adjustedSelfRect))
			{
				x = adjustedSelfRect.X;
				y = adjustedSelfRect.Y;
			}
			else
			{
				continue;
			}

			spawnPosition = new Point(x, y).ToWorldCoordinates(autoAddY: 16);

			// Ensure that this is not too close to a player.
			foreach (Player player in Main.ActivePlayers)
			{
				if (player.DistanceSQ(spawnPosition) <= minSqrDistanceFromPlayers)
				{
					goto Continue;
				}
			}

			// Ensure that this is not too close to an existing enemy.
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.DistanceSQ(spawnPosition) <= minSqrDistanceFromEnemies)
				{
					goto Continue;
				}
			}

			// Check for liquids if needed.
			if ((spawn.SkippedLiquids | spawn.RequiredLiquids) != 0)
			{
				if (!LiquidUtils.CheckAreaWithMasks(new Rectangle(x, y, sizeInTiles.X, sizeInTiles.Y), spawn.SkippedLiquids, spawn.RequiredLiquids))
				{
					continue;
				}
			}

			// As the last check, perform a floodfill loop to see if we can reach the encounter's center.
			const int FloodFillExtent = 8;
			var floodFillStart = new Vector2Int(x, y);
			var floodFillArea = new Vector4Int(
				Math.Max(xMin - FloodFillExtent, 1),
				Math.Max(yMin - FloodFillExtent, 1),
				Math.Min(xMax + FloodFillExtent, Main.maxTilesX - 2),
				Math.Min(yMax + FloodFillExtent, Main.maxTilesY - 2)
			);
			var floodFillRect = new Rectangle(
				floodFillArea.X,
				floodFillArea.Y,
				floodFillArea.Z - floodFillArea.X,
				floodFillArea.W - floodFillArea.Y
			);
			bool floodFillSuccess = false;

			foreach (GeometryUtils.FloodFill.Result step in new GeometryUtils.FloodFill(floodFillStart, floodFillRect))
			{
				// Inclusive.
				(int checkX1, int checkX2) = (step.Point.X + sizeOffsetX1, step.Point.X + sizeOffsetX2);
				(int checkY1, int checkY2) = (step.Point.Y + sizeOffsetY1, step.Point.Y + sizeOffsetY2);
				bool isPointFree = true;

				for (int checkX = checkX1; checkX <= checkX2; checkX++)
				{
					for (int checkY = checkY1; checkY <= checkY2; checkY++)
					{
						Tile tile = Main.tile[checkX, checkY];
						if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType])
						{
							isPointFree = false;
							goto CycleBreak;
						}
					}
				}

				CycleBreak: step.IsPointFree = isPointFree;

				if (step.Point.X == adjustedSpawnOrigin.X & step.Point.Y == adjustedSpawnOrigin.Y)
				{
					floodFillSuccess = true;
					break;
				}
			}

			// Failure, the origin point has never been reached.
			if (!floodFillSuccess)
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
}
