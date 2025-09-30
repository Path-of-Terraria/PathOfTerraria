using System.Collections.Generic;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.World.Passes;

internal class RemoveBeeLarvaeStep : AutoGenStep
{
	public override void Generate(GenerationProgress progress, GameConfiguration config)
	{
		for (int i = 100; i <= Main.maxTilesX - 100; i++)
		{
			for (int j = (int)Main.worldSurface; j < Main.maxTilesY - 200; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (tile.HasTile && tile.TileType == TileID.Larva)
				{
					tile.HasTile = false;
				}
			}
		}
	}

	public override int GenIndex(List<GenPass> tasks)
	{
		return tasks.Count - 1;
	}
}
