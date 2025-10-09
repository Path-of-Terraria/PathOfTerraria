using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Encounters;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Content.Conflux;
using PathOfTerraria.Utilities.Terraria;
using SubworldLibrary;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI.Chat;

#nullable enable

namespace PathOfTerraria.Common.Conflux;

/// <summary> This system spawns rifts in domain worlds. </summary>
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
			if (ShouldRiftsSpawnInWorld(SubworldSystem.Current))
			{
				SpawnRifts();
			}

			checkedRifts = true;
		}
	}

	public override void PostDrawInterface(SpriteBatch sb)
	{
		foreach (Projectile projectile in Main.ActiveProjectiles)
		{
			if (projectile.ModProjectile is not ConfluxRift { Activated: true } rift) { continue; }

			ChatManager.DrawColorCodedString(sb, FontAssets.MouseText.Value, $"Stability: {rift.Progress * 100:0.00}%", Main.ScreenSize.ToVector2() - new Vector2(256f, 64f), Color.White, 0f, default, Vector2.One);
		}
	}

	public static bool ShouldRiftsSpawnInWorld(Subworld world)
	{
		return world is MappingWorld and not RavencrestSubworld;
	}

	/// <summary> Attempts to spawn conflux rifts, returns the amount created. </summary>
	public static int SpawnRifts()
	{
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
}
