using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.MappingAreas.SwampAreaContent;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0060 // Remove unused parameter

internal static class SwampArenaGeneration
{
	public static void Generate(GenerationProgress progress, GameConfiguration configuration)
	{
		const int ShapeSize = 150;

		int x = SwampArea.LeftSpawn ? Main.maxTilesX - 300 : 300;
		int y = SwampArea.FloorY - 10;
		ShapeData data = new();
		WorldUtils.Gen(new Point(x, y), new Shapes.Circle(ShapeSize), new Actions.Clear().Output(data));

		for (int i = 0; i < 35; ++i)
		{
			float factor = SwampArea.Random.NextFloat(0.15f, 0.75f);
			float halfSize = ShapeSize * factor;
			var offset = SwampArea.Random.NextVector2CircularEdge(halfSize, halfSize).ToPoint16();
			WorldUtils.Gen(new Point(x, y), new Shapes.Circle((int)halfSize), Actions.Chain(new Modifiers.Offset(offset.X, offset.Y), new Actions.Clear().Output(data)));
		}

		WorldUtils.Gen(new Point(x, y), new ModShapes.InnerOutline(data, true), Actions.Chain(new Modifiers.Expand(1), new Modifiers.Dither(0.2f), 
			new Modifiers.Blotches(3, 0.3f), new Actions.PlaceTile(TileID.Mud)));
    }
}
