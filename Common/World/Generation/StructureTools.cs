using Terraria.DataStructures;

namespace PathOfTerraria.Common.World.Generation;

internal static class StructureTools
{
	public static Point16 GetSize(string structure, Mod mod = null)
	{
		Point16 size = new();
		StructureHelper.Generator.GetDimensions(structure, mod ?? ModContent.GetInstance<PoTMod>(), ref size);
		return size;
	}

	public static Point16 PlaceByOrigin(string structure, Point16 position, Vector2 origin, Mod mod = null, bool cullAbove = false)
	{
		mod ??= ModContent.GetInstance<PoTMod>();
		var dims = new Point16();
		StructureHelper.Generator.GetDimensions(structure, mod, ref dims);
		position = (position.ToVector2() - dims.ToVector2() * origin).ToPoint16();

		if (cullAbove)
		{
			CullLine(position, dims);
		}

		StructureHelper.Generator.GenerateStructure(structure, position, mod);
		return position;
	}

	private static void CullLine(Point16 position, Point16 dims)
	{
		for (int i = position.X; i < position.X + dims.X; ++i)
		{
			WorldGen.KillTile(i, position.Y - 1);
		}
	}
}
