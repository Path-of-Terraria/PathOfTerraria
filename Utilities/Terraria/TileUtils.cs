using System.Runtime.CompilerServices;
using PathOfTerraria.Utilities.Xna;

namespace PathOfTerraria.Utilities.Terraria;

/// <summary> Provides basic tile utilities.  </summary>
internal static class TileUtils
{
	/// <summary> The size of a tile in pixel units. </summary>
	public const float TileSizeInPixels = 16f;
	/// <summary> The size of a pixel in tile units. </summary>
	public const float PixelSizeInUnits = 1f / TileSizeInPixels;
}