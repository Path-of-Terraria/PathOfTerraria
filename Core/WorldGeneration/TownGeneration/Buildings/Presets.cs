using Terraria.ID;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Core.WorldGeneration.TownGeneration.Buildings;

public static class Presets
{
	public static Building BasicHut(int width, int height)
	{
		Point size = new Point(width, height);

		Building build = new Building(size, (size, pos) =>
		{
			ShapeData data = new();
			WorldUtils.Gen(pos, new Shapes.Rectangle(size.X, size.Y), Actions.Chain( // Clear & add walls
				new Actions.Clear().Output(data),
				new Actions.PlaceWall(WallID.Wood)));

			WorldUtils.Gen(pos, new ModShapes.InnerOutline(data, true), Actions.Chain( // Add tile walls & clear walls on border
				new Actions.ClearWall(),
				new Actions.SetTile(TileID.WoodBlock, true)
			));

			const int BeamOffset = 3;

			int yOff = 0;

			while (!WorldGen.SolidTile(pos.X + BeamOffset, pos.Y + size.Y + yOff)) 
			{
				WorldGen.PlaceTile(pos.X + BeamOffset, pos.Y + size.Y + yOff++, TileID.WoodenBeam, true, true);
			}

			yOff = 0;

			while (!WorldGen.SolidTile(pos.X + size.X - BeamOffset - 1, pos.Y + size.Y + yOff))
			{
				WorldGen.PlaceTile(pos.X + size.X - BeamOffset - 1, pos.Y + size.Y + yOff++, TileID.WoodenBeam, true, true);
			}
		});

		return build;
	}
}
