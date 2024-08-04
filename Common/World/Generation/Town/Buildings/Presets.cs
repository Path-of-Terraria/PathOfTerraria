using Terraria.DataStructures;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.World.Generation.Town.Buildings;

public static class Presets
{
	public static Building BasicHut(int width, int height, bool ruin)
	{
		var size = new Point(width, height);

		var build = new Building(size, (size, pos) =>
		{
			ShapeData data = new();
			WorldUtils.Gen(pos, new Shapes.Rectangle(size.X, size.Y), Actions.Chain( // Clear & add walls
				new Actions.ClearTile().Output(data),
				new Modifiers.Dither(ruin ? 0.2f : 0f),
				new Actions.PlaceWall(WallID.Wood)));

			WorldUtils.Gen(pos, new ModShapes.InnerOutline(data, true), Actions.Chain( // Add tile walls & clear walls on border
				new Modifiers.Dither(ruin ? 0.2f : 0f),
				new Actions.SetTile(TileID.WoodBlock, true)
			));

			AddBeams(size, pos, 3);
			AddDecor(pos, data, (TileID.SmallPiles, [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10]), (TileID.LargePiles, [7]));
		});

		return build;
	}

	private static void AddDecor(Point pos, ShapeData data, params (int type, int[] styles)[] types)
	{
		foreach (Point16 point in data.GetData())
		{
			if (WorldGen.genRand.NextBool(1))
			{
				(int type, int[] styles) = WorldGen.genRand.Next(types);
				WorldGen.PlaceObject(pos.X + point.X, pos.Y + point.Y, type, true, WorldGen.genRand.Next(styles));
			}
		}
	}

	private static void AddBeams(Point size, Point pos, int beamOffset)
	{
		int yOff = 0;

		while (!WorldGen.SolidTile(pos.X + beamOffset, pos.Y + size.Y + yOff))
		{
			WorldGen.PlaceTile(pos.X + beamOffset, pos.Y + size.Y + yOff++, TileID.WoodenBeam, true, true);
		}

		yOff = 0;

		while (!WorldGen.SolidTile(pos.X + size.X - beamOffset - 1, pos.Y + size.Y + yOff))
		{
			WorldGen.PlaceTile(pos.X + size.X - beamOffset - 1, pos.Y + size.Y + yOff++, TileID.WoodenBeam, true, true);
		}
	}
}
