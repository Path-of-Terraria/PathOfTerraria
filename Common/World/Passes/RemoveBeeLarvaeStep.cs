using System.Collections.Generic;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.World.Passes;

internal class RemoveBeeLarvaeStep : AutoGenStep
{
	public override void Generate(GenerationProgress progress, GameConfiguration config)
	{
		Range xRange = (Main.maxTilesX / 3)..(Main.maxTilesX - 100);

		if (Main.dungeonX < 0)
		{
			xRange = 100..(int)(Main.maxTilesX / 1.5f);
		}

		for (int i = xRange.Start.Value; i <= xRange.End.Value; i++)
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
