using PathOfTerraria.Content.Tiles.Maps;
using Terraria.DataStructures;
using Terraria.GameContent.Tile_Entities;
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

	/// <summary>
	/// Manually assigns a <see cref="TELogicSensor"/> to every player sensor tile that needs one.
	/// </summary>
	public static void ManuallyPopulatePlayerSensors()
	{
		for (int i = 10; i < Main.maxTilesX - 10; ++i)
		{
			for (int j = 10; j < Main.maxTilesY - 10; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (tile.TileType == TileID.LogicSensor && tile.TileFrameY == 36)
				{
					int id = TELogicSensor.Place(i, j);
					(TileEntity.ByID[id] as TELogicSensor).logicCheck = TELogicSensor.LogicCheckType.PlayerAbove;
				}
			}
		}
	}

	/// <summary>
	/// Cycles around the provided base point to try and find the nearest point that <paramref name="targetSize"/> can fit in.
	/// </summary>
	public static bool TryFindNearestFreePoint(Point basePoint, Point targetSize, Point maxSearchRadius, out Point result)
	{
		(int xOffset, int yOffset) = (0, 0);

		while (true) {
			(int xDiv, int xRem) = Math.DivRem(xOffset + 1, 2);
			(int yDiv, int yRem) = Math.DivRem(yOffset + 1, 2);
			int xStart = basePoint.X + (xDiv * (xRem == 0 ? 1 : -1));
			int yStart = basePoint.Y + (yDiv * (yRem == 0 ? 1 : -1));
			int xEnd = xStart + targetSize.X;
			int yEnd = yStart + targetSize.Y;

			bool CheckArea()
			{
				if (xStart < 0 || yStart < 0 || xEnd >= Main.maxTilesX || yEnd >= Main.maxTilesY)
				{
					return false;
				}

				for (int xx = xStart; xx <= xEnd; xx++)
				{
					for (int yy = yStart; yy <= yEnd; yy++)
					{
						if (WorldGen.SolidOrSlopedTile(xx, yy))
						{
							return false;
						}
					}
				}

				return true;
			}

			if (CheckArea())
			{
				result = new Point(xStart, yStart);
				return true;
			}

			if (++yOffset >= maxSearchRadius.Y)
			{
				yOffset = 0;
				if (++xOffset >= maxSearchRadius.X)
				{
					result = default;
					return false;
				}
			}
		}
	}
}
