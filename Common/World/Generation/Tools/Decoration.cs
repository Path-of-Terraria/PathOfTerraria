using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.World.Generation.Tools;

internal static class Decoration
{
	public static void GrowGrass(Point16 position, HashSet<Point16> grasses)
	{
		Tile tile = Main.tile[position];
		tile.TileType = TileID.Grass;
		grasses.Add(position);
	}

	public static void OnPurityGrass(Point16 position)
	{
		if (!WorldGen.genRand.NextBool(3))
		{
			WorldGen.PlaceTile(position.X, position.Y - 1, TileID.Plants);
		}
		else if (WorldGen.genRand.NextBool(6))
		{
			WorldGen.PlaceTile(position.X, position.Y - 1, TileID.Saplings);

			if (!WorldGen.GrowTree(position.X, position.Y - 1))
			{
				WorldGen.KillTile(position.X, position.Y - 1);
			}
		}
		else if (WorldGen.genRand.NextBool(4))
		{
			WorldGen.PlaceSmallPile(position.X, position.Y - 1, WorldGen.genRand.Next(10), 0);
		}
	}

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
