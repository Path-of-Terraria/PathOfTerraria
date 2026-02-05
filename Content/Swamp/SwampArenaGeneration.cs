using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Tiles.Maps.Swamp;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Content.Swamp;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0060 // Remove unused parameter

internal static class SwampArenaGeneration
{
	public static void Generate(GenerationProgress progress, GameConfiguration configuration)
	{
		const int ShapeSize = 150;

		int x = SwampArea.LeftSpawn ? Main.maxTilesX - 300 : 300;
		int y = SwampArea.FloorY - 10;

		int minX = SwampArea.LeftSpawn ? Main.maxTilesX - 500 : 20;
		int maxX = SwampArea.LeftSpawn ? Main.maxTilesX - 20 : 500;

		for (int i = minX; i < maxX; ++i)
		{
			for (int j = 20; j < SwampArea.FloorY + 60; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (tile.HasTile && tile.TileType is TileID.Mud or TileID.Stone)
				{
					continue;
				}

				GenPlacement.FastPlaceTile(i, j, ModContent.TileType<PurpleClouds>());
			}
		}

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
			new Modifiers.Blotches(3, 0.3f), new Actions.PlaceTile(TileID.Mud)));
    }
}
