using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.World.Utilities;
using PathOfTerraria.Content.Swamp.Tiles;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Content.Swamp;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0060 // Remove unused parameter

internal static class SwampArenaGeneration
{
	public const int ArenaWidth = 600;
	public const int HalfWidth = ArenaWidth / 2;

	public static Point16[] TraverseWalls { get; private set; }
	public static Point16[] TraverseNodes { get; private set; }
	public static Range WidthAtWaterHeight { get; private set; }

	public static void Generate(GenerationProgress progress, GameConfiguration configuration)
	{
		const int ShapeSize = 150;

		int x = SwampArea.LeftSpawn ? Main.maxTilesX - HalfWidth : HalfWidth;
		int y = SwampArea.FloorY - 10;

		GenerateClouds(out FastNoiseLite noise);
		Carve(progress, ShapeSize, x, y);
		SetupTraverse(x, y, noise, out Point16 center);
		BuildTraverse(noise, progress);
		Decorate(x, y, center);
	}

	private static void Carve(GenerationProgress progress, int ShapeSize, int x, int y)
	{
		ShapeData data = new();
		WorldUtils.Gen(new Point(x, y), new Shapes.Circle(ShapeSize), new Actions.Clear().Output(data));

		for (int i = 0; i < 35; ++i)
		{
			float factor = SwampArea.Random.NextFloat(0.15f, 0.75f);
			float halfSize = ShapeSize * factor;
			var offset = (SwampArea.Random.NextVector2CircularEdge(halfSize, halfSize) * 1.6f).ToPoint();

			if (offset.Y > 0)
			{
				offset.Y = (int)(offset.Y * 0.6f);
			}

			WorldUtils.Gen(new Point(x, y), new Shapes.Circle((int)((1 - factor) * ShapeSize)), Actions.Chain(new Modifiers.Offset(offset.X, offset.Y), new Actions.Clear().Output(data)));

			progress.Set(i / 34f);
		}

		WorldUtils.Gen(new Point(x, y), new ModShapes.InnerOutline(data, true), Actions.Chain(new Modifiers.Expand(1), new Modifiers.Dither(0.2f),
			new Modifiers.Blotches(3, 0.3f), new Actions.Clear()));
	}

	private static void Decorate(int x, int y, Point16 center)
	{
		int minX = x;
		int maxX = x;
		int minY = SwampArea.FloorY;

		while (!Main.tile[minX, minY].HasTile)
		{
			minX--;
		}

		while (!Main.tile[maxX, minY].HasTile)
		{
			maxX++;
		}

		int midX = (minX + maxX) / 2;
		float halfWidth = (maxX - minX) / 2f;

		WidthAtWaterHeight = minX..maxX;

		for (int i = minX; i < maxX; ++i)
		{
			int j = SwampArea.FloorY;

			while (!Main.tile[i, j].HasTile)
			{
				j++;
			}

			int depth = (int)MathHelper.Lerp(2, 12, 1 - Math.Abs(midX - i) / halfWidth);

			for (int k = j; k < j + depth; ++k)
			{
				Tile tile = Main.tile[i, k];

				if (!tile.HasTile)
				{
					break;
				}

				tile.TileType = (ushort)ModContent.TileType<DeepMoss>();
			}
		}

		const int Width = 50;

		center = new(center.X, center.Y + 16);
		FastNoiseLite noise = new(SwampArea.Random.Next());

		for (int i = center.X - Width; i < center.X + Width; ++i)
		{
			float distanceFromCenter = (int)Math.Abs(i - center.X) / (float)Width;
			int up = (int)MathHelper.Lerp(1, noise.GetNoise(i * 3, 0) * 6 + 8, 1 - distanceFromCenter);
			int down = (int)MathHelper.Lerp(0, noise.GetNoise(i * 3, 0) * 8 + 18, MathF.Sqrt(1 - distanceFromCenter));

			for (int j = center.Y - up; j < center.Y + down; ++j)
			{
				GenPlacement.FastPlaceTile(i, j, ModContent.TileType<DeepMoss>());
			}
		}
	}

	private static void BuildTraverse(FastNoiseLite noise, GenerationProgress progress)
	{
		HashSet<Point16> spawnPositions = [];

		foreach (Point16 start in TraverseWalls)
		{
			foreach (Point16 end in TraverseWalls)
			{
				if (start == end)
				{
					continue;
				}

				IEnumerable<Vector2> points = Tunnel.GeneratePoints([start.ToVector2(), SwampArea.Random.Next(TraverseNodes).ToVector2(), end.ToVector2()], 8, 3, 0).Select(x => x / 4f);

				foreach (Vector2 point in points)
				{
					spawnPositions.Add(point.ToPoint16());
				}
			}
		}

		HashSet<Point16> realPoints = [];
		int count = 0;

		foreach (Point16 position in spawnPositions)
		{
			float angle = SwampArea.Random.NextFloat(MathHelper.TwoPi);
			Vector2 realPos = position.ToVector2() * 4;

			if (!SwampArea.Random.NextBool(3))
			{
				float width = SwampArea.Random.NextFloat(9, 16);
				GenPlacement.GenerateLeaf(realPos, width, width * SwampArea.Random.NextFloat(1.2f, 1.8f), angle, (x, y, angle) => realPoints.Add(new(x, y)));
			}
			else
			{
				GenPlacement.GenOval(realPos, 30, SwampArea.Random.NextFloat(MathHelper.TwoPi), (x, y) => realPoints.Add(new(x, y)), (x, y) => noise.GetNoise(x * 3, y * 3) * 8);
			}

			progress.Set(count++ / (float)spawnPositions.Count);
		}

		List<Point16> pointsToUse = [.. realPoints];
		count = 0;

		foreach (Point16 point in CollectionsMarshal.AsSpan(pointsToUse))
		{
			if (Main.tile[point].WallType == WallID.None)
			{
				GenPlacement.FastPlaceWall(point.X, point.Y, ModContent.WallType<DeepMossWall>());
			}

			progress.Set(count++ / (float)pointsToUse.Count);
		}
	}

	private static void SetupTraverse(int x, int y, FastNoiseLite noise, out Point16 centerPosition)
	{
		const int WallPointsCount = 7;
		const int OpenPointsCount = 4;

		List<Point16> workingPoints = [];
		List<Vector2> workingDirections = [];

		for (int i = 0; i < WallPointsCount; ++i)
		{
			Vector2 pos = new(x, y);
			Vector2 dir;

			do
			{
				dir = (!SwampArea.LeftSpawn ? new Vector2(-1.5f, 0) : new Vector2(1.5f, 0)).RotatedByRandom(MathHelper.Pi * 0.85f);
			} while (workingDirections.Any(x => Vector2.DistanceSquared(dir, x) < 0.5f));

			while (!Collision.SolidCollision(pos.ToWorldCoordinates(), 32, 32))
			{
				pos += dir;
			}

			workingPoints.Add(pos.ToPoint16());
			workingDirections.Add(dir);

			WorldGen.TileRunner((int)pos.X, (int)pos.Y, 36, 160, ModContent.TileType<DeepMoss>(), false, dir.X * 3, dir.Y * 3);
		}

		TraverseWalls = [.. workingPoints];
		workingPoints.Clear();
		centerPosition = new(x, SwampArea.FloorY - 4);
		var center = centerPosition.ToVector2();
		int repeats = 0;

		for (int i = 0; i < OpenPointsCount; ++i)
		{
			Vector2 position = i == 0 ? center : center + new Vector2(0, (-120 - repeats * 0.01f) * SwampArea.Random.NextFloat(0.7f, 1f)).RotatedByRandom(MathHelper.PiOver2);
			position.X = MathHelper.Clamp(position.X, 60, Main.maxTilesX - 60);

			if (i > 0 && (Collision.SolidCollision(position * 16 - new Vector2(-100), 200, 200) || workingPoints.Any(x => x.ToVector2().DistanceSQ(position) < 110 * 110)))
			{
				repeats++;
				i--;
				continue;
			}

			workingPoints.Add(position.ToPoint16());

			GenPlacement.GenOval(position, 80, SwampArea.Random.NextFloat(MathHelper.TwoPi), ModContent.WallType<PurpleCloudWall>(), (x, y) => noise.GetNoise(x * 3, y * 3) * 8, true);
		}

		TraverseNodes = [.. workingPoints];
	}

	private static void GenerateClouds(out FastNoiseLite noise)
	{
		int minX = SwampArea.LeftSpawn ? Main.maxTilesX - ArenaWidth : 4;
		int maxX = SwampArea.LeftSpawn ? Main.maxTilesX - 4 : ArenaWidth;

		noise = new(SwampArea.Random.Next());
		noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
		noise.SetFrequency(0.02f);
		noise.SetDomainWarpAmp(77);
		noise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2Reduced);
		noise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Hybrid);

        for (int j = 4; j < SwampArea.FloorY + 30; ++j)
		{
			float x = minX + GetNoiseOffset(noise, 0, j, false);

            for (int i = (int)x; i < maxX + GetNoiseOffset(noise, maxX, j, true); ++i)
			{
				Tile tile = Main.tile[i, j];

				if (tile.HasTile && tile.TileType is TileID.Mud or TileID.Stone)
				{
					continue;
				}

				GenPlacement.FastPlaceTile(i, j, ModContent.TileType<PurpleClouds>());
			}

            x = minX + GetNoiseOffset(noise, 16000, j, false);

            for (int i = (int)MathF.Max(x - 15, 4); i < MathF.Min(maxX + GetNoiseOffset(noise, maxX + 16000, j, true) + 15, Main.maxTilesX - 4); ++i)
            {
                GenPlacement.FastPlaceWall(i, j, ModContent.WallType<PurpleCloudWall>());
            }
        }

        for (int i = (int)MathF.Max(minX - 60, 4); i < MathF.Min(maxX + 60, Main.maxTilesX - 4); ++i)
        {
			int j = SwampArea.FloorY + 20;

			while ((Main.tile[i, j].TileType != ModContent.TileType<PurpleClouds>()) && j > 10)
			{
				j--;
			}

			if (Main.tile[i, j].TileType != ModContent.TileType<PurpleClouds>() || SwampArea.FloorY - j > 40)
			{
				continue;
			}

			if (i < ArenaWidth / 2 || i > Main.maxTilesX - ArenaWidth / 2)
			{
				continue;
			}
			
			for (int y = j - 18; y <= j; ++y)
			{
				Tile tile = Main.tile[i, y];

				if (WorldUtilities.SolidOrActuatedTile(i, y))
				{
					tile.HasTile = false;
				}

				tile.WallType = (ushort)(SwampArea.Random.NextFloat() < Utils.GetLerpValue(j - 10, j - 6, y, true) ? ModContent.WallType<DeepMossWall>() 
					: ModContent.WallType<PurpleCloudWall>());

				if (y == j && Main.tile[i, y + 1].HasTile)
				{
					tile.HasTile = true;
					tile.TileType = (ushort)ModContent.TileType<SwampGrass>();
				}
				else if (i is < ArenaWidth - 53 and > ArenaWidth - 58 || (i > Main.maxTilesX - ArenaWidth + 53 && i < Main.maxTilesX - ArenaWidth + 58))
				{
					SwampArea.BlockerPositions.Add(new Point16(i, y));
				}
			}
        }
    }

	private static int GetNoiseOffset(FastNoiseLite noise, int i, int j, bool leftSpawn)
	{
		if (leftSpawn == SwampArea.LeftSpawn)
		{
			return i;
		}

		float x = i;
		float y = j;
        noise.DomainWarp(ref x, ref y);
		int offset = (int)(noise.GetNoise(x, y) * 15);

		const float Divisor = 4f;
		const int Total = 300;
		const float RealTotal = Total / Divisor;

		if (y < Total)
		{
			if (leftSpawn)
			{
				offset -= (int)MathF.Pow(RealTotal - (y / Divisor), 1.2f);
			}
			else
			{
				offset += (int)MathF.Pow(RealTotal - (y / Divisor), 1.2f);
			}
		}

		return offset;
	}
}
