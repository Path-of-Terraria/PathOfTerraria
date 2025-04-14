using PathOfTerraria.Content.Tiles.Maps;
using Terraria.ID;

namespace PathOfTerraria.Common.World.Generation.Tools;

internal class GenerationUtilities
{
	/// <summary>
	/// Manually assigns a <see cref="Chest"/> to every chest that needs one.
	/// </summary>
	public static void ManuallyPopulateChests()
	{
		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (tile.HasTile && TileID.Sets.IsAContainer[tile.TileType] && TileID.Sets.BasicChest[tile.TileType])
				{
					Chest.CreateChest(i, j);
				}
			}
		}
	}
}
