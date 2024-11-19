using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Content.Tiles.BossDomain;
using PathOfTerraria.Content.Walls;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.RealtimeGen.Generation;

/// <summary>
/// Handles the small chasm generated in realtime by breaking a 3rd shadow orb.
/// </summary>
internal static class EoWChasmGeneration
{
	internal static void SpawnChasm(int i, int j)
	{
		// Sets up variables for use later.
		int dir = Main.rand.NextBool() ? -1 : 1; // Direction of the chasm
		int depth = Main.rand.Next(40, 50); // Length of the chasm
		float addY = 0; // Vertical position offset based on noise

		FastNoiseLite verticalNoise = new(Main.rand.Next());
		verticalNoise.SetFrequency(0.05f);
		verticalNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);

		PriorityQueue<RealtimeStep, float> steps = new();

		HashSet<Point16> tiles = [];
		HashSet<Point16> walls = [];
		Point16 portalPosition = default;

		// Modified distance method that is heavily y-biased so it looks like the chasm moves horizontally first.
		float QuickDistance(int x, int y)
		{
			return MathF.Pow(j - y, 2) + MathF.Pow(i - x, 2) * 0.07f;
		}

		// Determines basic shape, tunnel and sloping.
		// This is also where portal position is determined.
		for (int l = 0; l < depth; ++l)
		{
			int x = i + dir * l;
			float wallOffTop = verticalNoise.GetNoise(l, 0) * 1.2f;
			float wallOffBottom = verticalNoise.GetNoise(l, 0) * 1.2f;

			float emptyOffTop = verticalNoise.GetNoise(l, 0) * 1.2f;
			float emptyOffBottom = verticalNoise.GetNoise(l, 0) * 1.2f;

			for (int k = (int)(-8 - wallOffTop * 3); k < 8 + wallOffBottom; ++k)
			{
				int y = j + k;

				float wallStart = MathF.Min(-3 - emptyOffTop * 3, -2);
				float wallEnd = MathF.Max(3 + emptyOffBottom * 3, 2);

				if (k >= wallStart && k < wallEnd && l < depth - 4)
				{
					// Digs tunnel
					steps.Enqueue(RealtimeSteps.KillTile(x, y), QuickDistance(x, y));
					walls.Add(new Point16(x, y));
				}
				else
				{
					bool isMalaise = l > depth / 2;
					int cutoffStart = depth / 2 - 4;

					if (l >= cutoffStart)
					{
						isMalaise = Main.rand.NextBool(Math.Max(5 - (l - cutoffStart), 1));
					}

					// Places tiles
					steps.Enqueue(new RealtimeStep((int i, int j) =>
					{
						int type = isMalaise ? ModContent.TileType<WeakMalaise>() : TileID.Ebonstone;
						WorldGen.PlaceTile(x, y, type, true, true);

						Tile tile = Main.tile[x, y];
						tile.Slope = SlopeType.Solid;

						if (Main.netMode != NetmodeID.SinglePlayer)
						{
							NetMessage.SendTileSquare(-1, i, j);
						}

						return tile.TileType == type;
					}, new Point16(x, y)), QuickDistance(x, y));

					tiles.Add(new Point16(x, y));
				}

				if (k == 0 && l == depth - 5)
				{
					portalPosition = new Point16(x, y);
				}
			}

			addY += verticalNoise.GetNoise(l, 0) * 1.2f;

			if (addY > 1)
			{
				j += (int)addY;
				addY -= (int)addY;
			}
		}

		FastNoiseLite wallNoise = new();
		wallNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
		wallNoise.SetFrequency(0.020f);
		wallNoise.SetFractalType(FastNoiseLite.FractalType.PingPong);

		List<RealtimeStep> lateSteps = [];

		// Adds in walls and slopes tiles.
		foreach (Point16 point in tiles)
		{
			bool ContainsPoint(int x, int y)
			{
				return tiles.Contains(new Point16(x, y)) || walls.Contains(new Point16(x, y));
			}

			bool openAdjacent = !ContainsPoint(point.X, point.Y - 1) || !ContainsPoint(point.X, point.Y + 1) || ContainsPoint(point.X - 1, point.Y) ||
				!ContainsPoint(point.X + 1, point.Y);

			// If no adjacent tile is empty, place a wall here.
			if (!openAdjacent)
			{
				Tile tile = Main.tile[point];
				int wallType = wallNoise.GetNoise(point.X, point.Y) > 0.4f ? ModContent.WallType<MalaiseWall>() : WallID.EbonstoneUnsafe;

				steps.Enqueue(RealtimeSteps.PlaceWall(point.X, point.Y, wallType, true), QuickDistance(point.X, point.Y));
			}

			if (!tiles.Contains(new Point16(point.X, point.Y + 1)) && Main.rand.NextBool(3))
			{
				lateSteps.Add(RealtimeSteps.PlaceStalactite(point.X, point.Y + 1, Main.rand.NextBool(4), 5, null, true));
			}
			else
			{
				lateSteps.Add(RealtimeSteps.SmoothSlope(point.X, point.Y, true));
			}
		}
		
		// Adds in walls.
		foreach (Point16 point in walls)
		{
			Tile tile = Main.tile[point];
			int wallType = wallNoise.GetNoise(point.X, point.Y) > 0.35f ? ModContent.WallType<MalaiseWall>() : WallID.EbonstoneUnsafe;

			steps.Enqueue(RealtimeSteps.PlaceWall(point.X, point.Y, wallType), QuickDistance(point.X, point.Y));
		}

		// Spawns portal.
		lateSteps.Add(new RealtimeStep((x, y) =>
		{
			float xPos = x * 16 - 6;

			if (dir == 1)
			{
				xPos += 24;
			}

			int proj = Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), xPos, y * 16 + 8, 0, 0, ModContent.ProjectileType<EoWPortal>(), 0, 0, Main.myPlayer);

			Main.projectile[proj].rotation = dir * MathHelper.PiOver2 - MathHelper.PiOver2;
			Main.projectile[proj].netUpdate = true;
			return true;
		}, portalPosition));

		// Finish generation and add all steps.
		List<RealtimeStep> useSteps = [];

		while (steps.Count > 0)
		{
			useSteps.Add(steps.Dequeue());
		}

		useSteps.AddRange(lateSteps);
		RealtimeGenerationSystem.AddAction(new RealtimeGenerationAction(useSteps, 0.002f));
	}
}
