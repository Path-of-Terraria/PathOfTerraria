using PathOfTerraria.Content.Tiles.Maps;
using Terraria.ID;

namespace PathOfTerraria.Common.Subworlds.MappingAreas;

internal class ShrineFunctionality
{
	public const int ShrineDenominator = 5;

	/// <summary>
	/// Gives all <see cref="BaseShrine"/>s an associated <see cref="BaseShrine.ShrineTileEntity"/>, as this is not done automatically by worldgen.
	/// </summary>
	public static void PopulateShrines()
	{
		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (tile.TileType >= TileID.Count && ModContent.GetModTile(tile.TileType) is BaseShrine shrine && tile.TileFrameX % 36 == 0 && tile.TileFrameY == 0)
				{
					ModContent.GetInstance<BaseShrine.ShrineTileEntity>().Place(i, j);
				}
			}
		}
	}
}
