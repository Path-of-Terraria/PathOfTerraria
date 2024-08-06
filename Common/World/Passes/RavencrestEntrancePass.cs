using Microsoft.Build.Framework;
using System.Collections.Generic;
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

	public static bool FindPlacement(int x, int y, out Point16 position)
	{
		position = new Point16(x, y);

		while (!WorldGen.SolidTile(x, ++y))
		{
		}

		Tile tile = Main.tile[x, y];

		if (tile.TileType != TileID.Grass)
		{
			return false;
		}

		int averageHeight = AverageHeights(x, y, 75, 16, 10, out bool valid, [TileID.Cloud, TileID.RainCloud, TileID.Ebonstone, TileID.Crimstone],
			TileID.Grass, TileID.ClayBlock, TileID.Dirt, TileID.Iron, TileID.Copper, TileID.Lead, TileID.Tin);

		if (valid && averageHeight < 15)
		{
			position = new Point16(x, y - 38);
			return true;
		}

		return false;
	}

	private static Point16 FindPlacement()
	{
		while (true)
		{
			int x = WorldGen.genRand.Next(150, Main.maxTilesX - 150);

			while (Math.Abs(x - Main.spawnTileX) < 150)
			{
				x = WorldGen.genRand.Next(150, Main.maxTilesX - 150);
			}

			int y = (int)(Main.worldSurface * 0.35f);

			while (!WorldGen.SolidTile(x, ++y))
			{
			}

			Tile tile = Main.tile[x, y];

			if (tile.TileType != TileID.Grass)
			{
				continue;
			}

			int averageHeight = AverageHeights(x, y, 73, 20, 4, out bool valid, [TileID.Cloud, TileID.RainCloud, TileID.Ebonstone, TileID.Crimstone], 
				TileID.Grass, TileID.ClayBlock, TileID.Dirt, TileID.Iron, TileID.Copper, TileID.Lead, TileID.Tin, TileID.Stone);

			if (valid && Math.Abs(averageHeight) < 5)
			{
				return new Point16(x, y - 38 + averageHeight);
			}
		}
	}

	private static int AverageHeights(int x, int y, int width, int validSkips, int depth, out bool valid, int[] hardAvoidIds, params int[] allowedIds)
	{
		int heights = 0;
		int avgDepth = 0;
		int skips = 0;

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

			if (hardAvoidIds.Contains(Main.tile[i, useY].TileType))
			{
				valid = false;
				return -1;
			}

			if (!allowedIds.Contains(Main.tile[i, useY].TileType) && ++skips > validSkips)
			{
				valid = false;
				return -1;
			}
		}

		int realWidth = width / 2 * 2;
		valid = avgDepth / realWidth > depth;
		return heights / realWidth;
	}

	public override int GenIndex(List<GenPass> tasks)
	{
		return tasks.FindIndex(x => x.Name == "Spawn Point") + 1;
	}
}
