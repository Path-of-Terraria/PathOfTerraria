using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.World.Utilities;
using PathOfTerraria.Content.Tiles.Maps.Swamp;
using PathOfTerraria.Content.Walls;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Content.Swamp;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0060 // Remove unused parameter

internal static class SwampArenaGeneration
{
	public const int ArenaWidth = 600;

	public static void Generate(GenerationProgress progress, GameConfiguration configuration)
	{
		const int ShapeSize = 150;

		int x = SwampArea.LeftSpawn ? Main.maxTilesX - 300 : 300;
		int y = SwampArea.FloorY - 10;

		GenerateClouds();

		ShapeData data = new();
		WorldUtils.Gen(new Point(x, y), new Shapes.Circle(ShapeSize), new Actions.Clear().Output(data));

		for (int i = 0; i < 35; ++i)
		{
			float factor = SwampArea.Random.NextFloat(0.15f, 0.75f);
			float halfSize = ShapeSize * factor;
			var offset = (SwampArea.Random.NextVector2CircularEdge(halfSize, halfSize) * 1.6f).ToPoint16();
			WorldUtils.Gen(new Point(x, y), new Shapes.Circle((int)((1 - factor) * ShapeSize)), Actions.Chain(new Modifiers.Offset(offset.X, offset.Y), new Actions.Clear().Output(data)));
		}

		WorldUtils.Gen(new Point(x, y), new ModShapes.InnerOutline(data, true), Actions.Chain(new Modifiers.Expand(1), new Modifiers.Dither(0.2f),
			new Modifiers.Blotches(3, 0.3f), new Actions.Clear()));
	}

	private static void GenerateClouds()
	{
		int minX = SwampArea.LeftSpawn ? Main.maxTilesX - ArenaWidth : 4;
		int maxX = SwampArea.LeftSpawn ? Main.maxTilesX - 4 : ArenaWidth;

		FastNoiseLite noise = new(SwampArea.Random.Next());
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

            x = minX + GetNoiseOffset(noise, 0, j + 16000, false);

            for (int i = (int)MathF.Max(x - 15, 4); i < MathF.Min(maxX + GetNoiseOffset(noise, maxX, j + 16000, true) + 15, Main.maxTilesX - 4); ++i)
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
			
			for (int y = j - 15; y <= j; ++y)
			{
				Tile tile = Main.tile[i, y];

				if (WorldUtilities.SolidOrActuatedTile(i, y))
				{
					tile.HasTile = false;
				}

				tile.WallType = (ushort)ModContent.WallType<PurpleCloudWall>();

				if (y == j && Main.tile[i, y + 1].HasTile)
				{
					tile.HasTile = true;
					tile.TileType = (ushort)ModContent.TileType<SwampGrass>();
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
        return (int)(noise.GetNoise(x, y) * 15);
	}
}
