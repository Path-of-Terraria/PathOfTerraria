using System.Collections.Generic;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.SkeleDomain;

internal class DungeonConversion
{
	public enum DungeonColor
	{
		Blue,
		Green,
		Pink,
		Count
	}

#pragma warning disable IDE0060 // Remove unused parameter
	internal static void Convert(GenerationProgress progress, GameConfiguration configuration)
#pragma warning restore IDE0060 // Remove unused parameter
	{
		var color = (DungeonColor)Main.rand.Next((int)DungeonColor.Count);

		if (color == DungeonColor.Blue)
		{
			return;
		}

		Dictionary<int, int> WallToAlt = new Dictionary<int, int>()
		{
			{ WallID.BlueDungeon, color == DungeonColor.Pink ? WallID.PinkDungeonUnsafe : WallID.GreenDungeonUnsafe },
			{ WallID.BlueDungeonUnsafe, color == DungeonColor.Pink ? WallID.PinkDungeonUnsafe : WallID.GreenDungeonUnsafe },
			{ WallID.BlueDungeonSlab, color == DungeonColor.Pink ? WallID.PinkDungeonSlabUnsafe : WallID.GreenDungeonSlabUnsafe },
			{ WallID.BlueDungeonSlabUnsafe, color == DungeonColor.Pink ? WallID.PinkDungeonSlabUnsafe : WallID.GreenDungeonSlabUnsafe },
			{ WallID.BlueDungeonTile, color == DungeonColor.Pink ? WallID.PinkDungeonTileUnsafe : WallID.GreenDungeonTileUnsafe },
			{ WallID.BlueDungeonTileUnsafe, color == DungeonColor.Pink ? WallID.PinkDungeonTileUnsafe : WallID.GreenDungeonTileUnsafe }
		};

		for (int i = 2; i < Main.maxTilesX - 2; i++)
		{
			for (int j = 2; j < Main.maxTilesY - 2; j++)
			{
				Tile tile = Main.tile[i, j];

				if (tile.TileType == TileID.BlueDungeonBrick) // Brick
				{
					tile.TileType = color == DungeonColor.Pink ? TileID.PinkDungeonBrick : TileID.GreenDungeonBrick;
				}
				else if (tile.TileType == TileID.Platforms && tile.TileFrameY == 6 * 18) // Platforms
				{
					tile.TileFrameY = (short)(color == DungeonColor.Pink ? 7 * 18 : 8 * 18);
				} // Bookcase
				else if (tile.TileType == TileID.Bookcases && tile.TileFrameX >= 54 && tile.TileFrameX < 108 && tile.TileFrameY < 72)
				{
					tile.TileFrameX += (short)(color == DungeonColor.Pink ? 108 : 54);
				}
				else if (tile.TileType == TileID.WorkBenches && tile.TileFrameX >= 396 && tile.TileFrameX < 432) // Workbenches
				{
					tile.TileFrameX += (short)(color == DungeonColor.Pink ? 72 : 36);
				}
				else if (tile.TileType == TileID.Chairs && tile.TileFrameY >= 520 && tile.TileFrameX < 560) // Chairs
				{
					tile.TileFrameY += (short)(color == DungeonColor.Pink ? 80 : 40);
				}
				else if (tile.TileType == TileID.Tables && tile.TileFrameY >= 540 && tile.TileFrameX < 594 && tile.TileFrameY < 38) // Tables
				{
					tile.TileFrameX += (short)(color == DungeonColor.Pink ? 108 : 54);
				}
				else if (tile.TileType == TileID.Chandeliers && tile.TileFrameY >= 1458 && tile.TileFrameY < 1512 && tile.TileFrameX < 108) // Chandeliers
				{
					tile.TileFrameY += (short)(color == DungeonColor.Pink ? 108 : 54);
				}
				else if (tile.TileType == TileID.Statues && tile.TileFrameX >= 1656 && tile.TileFrameX < 1692 && // Vases
					(tile.TileFrameY <= 54 || tile.TileFrameY >= 162 && tile.TileFrameY < 216))
				{
					tile.TileFrameX += (short)(color == DungeonColor.Pink ? 72 : 36);
				}
				else if (tile.TileType == TileID.GrandfatherClocks && tile.TileFrameX >= 1080 && tile.TileFrameX < 1116) // Clocks
				{
					tile.TileFrameX += (short)(color == DungeonColor.Pink ? 72 : 36);
				}
				else if (tile.TileType == TileID.Benches && tile.TileFrameX >= 324 && tile.TileFrameX < 378 && tile.TileFrameY < 36) // Benches
				{
					tile.TileFrameX += (short)(color == DungeonColor.Pink ? 108 : 54);
				}
				else if (tile.TileType == TileID.Pianos && tile.TileFrameX >= 594 && tile.TileFrameX < 468 && tile.TileFrameY < 36) // Pianos
				{
					tile.TileFrameX += (short)(color == DungeonColor.Pink ? 108 : 54);
				}
				else if (tile.TileType == TileID.Candles && tile.TileFrameY >= 22 && tile.TileFrameY < 44) // Candles
				{
					tile.TileFrameY += (short)(color == DungeonColor.Pink ? 44 : 22);
				}
				else if (tile.TileType == TileID.Lamps && tile.TileFrameY >= 1298 && tile.TileFrameY < 1350 && tile.TileFrameX < 34) // Lamps
				{
					tile.TileFrameY += (short)(color == DungeonColor.Pink ? 108 : 54);
				}
				else if (tile.TileType == TileID.ClosedDoor && tile.TileFrameY >= 864 && tile.TileFrameY < 918 && tile.TileFrameX < 72) // Door (C)
				{
					tile.TileFrameY += (short)(color == DungeonColor.Pink ? 108 : 54);
				}
				else if (tile.TileType == TileID.OpenDoor && tile.TileFrameY >= 864 && tile.TileFrameY < 918 && tile.TileFrameX < 54) // Door (0)
				{
					tile.TileFrameY += (short)(color == DungeonColor.Pink ? 108 : 54);
				}

				if (WallToAlt.TryGetValue(tile.WallType, out int id))
				{
					tile.WallType = (ushort)id;
				}
			}

			progress.Value = i / (Main.maxTilesX - 4f);
		}
	}
}