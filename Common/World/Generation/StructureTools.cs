using StructureHelper;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.World.Generation;

internal static class StructureTools
{
	/// <summary>
	/// Shorthand for calling <see cref="StructureHelper.API.Generator.GetStructureDimensions(string, Mod, bool)"/> with <see cref="PoTMod.Instance"/>.
	/// </summary>
	/// <param name="structure">Path to the structure. Formatted like "Assets/Structures/XX/Structure".</param>
	/// <param name="mod"></param>
	/// <returns></returns>
	public static Point16 GetSize(string structure, Mod mod = null)
	{
		return StructureHelper.API.Generator.GetStructureDimensions(structure, mod ?? PoTMod.Instance);
	}

	/// <summary>
	/// Places a structure at the given position and origin.
	/// </summary>
	/// <param name="structure">Path to the structure, NOT including mod name. Formatted like "Assets/Structures/XX/Structure".</param>
	/// <param name="position">Position of the structure, according to origin.</param>
	/// <param name="origin">Origin of the structure; (0, 0) is top-left, (1, 1) is bottom-right.</param>
	/// <param name="mod">Mod to reference. Usually null for <see cref="PoTMod.Instance"/>.</param>
	/// <param name="cullAbove">Whether or not to cull tiles above the structure. Used to cut down trees.</param>
	/// <param name="noSync">Stops StructureHelper from sending a sync packet if desired.</param>
	/// <returns></returns>
	public static Point16 PlaceByOrigin(string structure, Point16 position, Vector2 origin, Mod mod = null, bool cullAbove = false, 
		bool noSync = false, GenFlags flags = GenFlags.None)
	{
		mod ??= ModContent.GetInstance<PoTMod>();
		Point16 dims = GetSize(structure);
		position = (position.ToVector2() - dims.ToVector2() * origin).ToPoint16();

		if (cullAbove)
		{
			CullLine(position, dims);
		}

		int oldVal = Main.netMode;

		if (noSync)
		{
			Main.netMode = NetmodeID.SinglePlayer;
		}

		StructureHelper.API.Generator.GenerateStructure(structure, position, mod, flags: flags);
		Main.netMode = oldVal;
		return position;
	}

	/// <summary>
	/// Used to remove trees above structures in certain areas.
	/// </summary>
	/// <param name="position"></param>
	/// <param name="dims"></param>
	internal static void CullLine(Point16 position, Point16 dims)
	{
		for (int i = position.X; i < position.X + dims.X; ++i)
		{
			WorldGen.KillTile(i, position.Y - 1);
		}
	}

	/// <summary>
	/// Determines flatness & depth placement of an area. Returns average heights; use <paramref name="valid"/> to check if the space is valid.<br/>
	/// This needs a lot of tweaking to get perfect.
	/// </summary>
	/// <param name="x">Left of the area.</param>
	/// <param name="y">Bottom of the area.</param>
	/// <param name="width">Width of the area.</param>
	/// <param name="validSkips">How many times non-<paramref name="allowedIds"/> tiles can be scanned before invalidating the area.</param>
	/// <param name="depth">The desired average depth for the area.</param>
	/// <param name="valid">Whether the area could be valid.</param>
	/// <param name="hardAvoidIds">If a tile of any of these types are scanned, the area is automatically invalid.</param>
	/// <param name="allowedIds">Tiles that do not increment skips when scanned.</param>
	/// <returns>Average height.</returns>
	public static int AverageHeights(int x, int y, int width, int validSkips, int depth, out bool valid, int[] hardAvoidIds, int[] allowedIds)
	{
		int heights = 0;
		int avgDepth = 0;
		int skips = 0;

		if (x < 0 || x > Main.maxTilesX)
		{
			valid = false;
			return 0;
		}

		for (int i = x - width / 2; i < x + width / 2; i++)
		{
			int useY = y;

			if (WorldGen.SolidTile(i, useY))
			{
				while (WorldGen.SolidTile(i, --useY))
				{
				}

				useY++;
			}
			else
			{
				while (!WorldGen.SolidTile(i, ++useY))
				{
				}
			}

			int heightDif = useY - y;
			heights += heightDif;

			int digY = useY;

			while (WorldGen.SolidTile(i, ++digY) && digY < useY + depth * 1.1f)
			{
			}

			avgDepth += digY - useY + heightDif;

			if (hardAvoidIds.Length > 0 && hardAvoidIds.Contains(Main.tile[i, useY].TileType))
			{
				valid = false;
				return -1;
			}

			if (allowedIds.Length > 0 && !allowedIds.Contains(Main.tile[i, useY].TileType) && ++skips > validSkips)
			{
				valid = false;
				return -1;
			}
		}

		int realWidth = width / 2 * 2;
		valid = avgDepth / realWidth > depth;
		return heights / realWidth;
	}
}
