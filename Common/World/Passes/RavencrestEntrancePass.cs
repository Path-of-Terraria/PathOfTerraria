﻿using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.World.Passes;

internal class RavencrestEntrancePass : AutoGenStep
{
	public override void Generate(GenerationProgress progress, GameConfiguration config)
	{
		Point16 pos = FindPlacement();
		StructureHelper.Generator.GenerateStructure("Assets/Structures/RavencrestEntrance", pos, Mod);
	}

	private static Point16 FindPlacement()
	{
		while (true)
		{
			int x = WorldGen.genRand.Next(150, Main.maxTilesX - 150);
			int y = (int)(Main.worldSurface * 0.35f);

			while (!WorldGen.SolidTile(x, ++y))
			{
			}

			Tile tile = Main.tile[x, y];

			if (tile.TileType != TileID.Grass)
			{
				continue;
			}

			int averageHeight = AverageHeights(x, y, 75, 10, out bool valid, [TileID.Cloud, TileID.RainCloud, TileID.Ebonstone, TileID.Crimstone], 
				TileID.Grass, TileID.ClayBlock, TileID.Dirt, TileID.Iron, TileID.Copper, TileID.Lead, TileID.Tin);

			if (valid && averageHeight < 15)
			{
				return new Point16(x, y - 38);
			}
		}
	}

	private static int AverageHeights(int x, int y, int width, int validSkips, out bool valid, int[] hardAvoidIds, params int[] allowedIds)
	{
		int heights = 0;
		int skips = 0;

		for (int i = x - width / 2; i < x + width / 2; i++)
		{
			int useY = y;

			if (WorldGen.SolidTile(i, useY))
			{
				while (WorldGen.SolidTile(i, --useY))
				{
				}
			}
			else
			{
				while (!WorldGen.SolidTile(i, ++useY))
				{
				}
			}

			heights += useY - y;

			if (hardAvoidIds.Contains(Main.tile[i, useY].TileType))
			{
				valid = false;
				return -1;
			}

			if (allowedIds.Contains(Main.tile[i, useY].TileType) && ++skips > validSkips)
			{
				valid = false;
				return -1;
			}
		}

		valid = true;
		return heights / (width / 2 * 2);
	}

	public override int GenIndex(List<GenPass> tasks)
	{
		return tasks.FindIndex(x => x.Name == "Spawn Point") + 1;
	}
}
