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

		var WallToAlt = new Dictionary<int, int>()
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
				Tile t = Main.tile[i, j];

				if (t.TileType == TileID.BlueDungeonBrick) // Brick
				{
					t.TileType = color == DungeonColor.Pink ? TileID.PinkDungeonBrick : TileID.GreenDungeonBrick;
				}
				else if (t.TileType == TileID.Platforms && t.TileFrameY == 6 * 18) // Platforms
				{
					t.TileFrameY = (short)(color == DungeonColor.Pink ? 7 * 18 : 8 * 18);
				}
				else if (t.TileType == TileID.Bookcases && t.TileFrameX >= 54 && t.TileFrameX < 108 && t.TileFrameY < 72) // Bookcase
				{
					t.TileFrameX += (short)(color == DungeonColor.Pink ? 108 : 54);
				}
				else if (t.TileType == TileID.WorkBenches && t.TileFrameX >= 396 && t.TileFrameX < 432) // Workbenches
				{
					t.TileFrameX += (short)(color == DungeonColor.Pink ? 72 : 36);
				}
				else if (t.TileType == TileID.Chairs && t.TileFrameY >= 520 && t.TileFrameX < 560) // Chairs
				{
					t.TileFrameY += (short)(color == DungeonColor.Pink ? 80 : 40);
				}
				else if (t.TileType == TileID.Tables && t.TileFrameX >= 540 && t.TileFrameX < 594 && t.TileFrameY < 38) // Tables
				{
					t.TileFrameX += (short)(color == DungeonColor.Pink ? 108 : 54);
				}
				else if (t.TileType == TileID.Chandeliers && t.TileFrameY >= 1458 && t.TileFrameY < 1512 && t.TileFrameX < 108) // Chandeliers
				{
					t.TileFrameY += (short)(color == DungeonColor.Pink ? 108 : 54);
				}
				else if (t.TileType == TileID.Statues && t.TileFrameX >= 1656 && t.TileFrameX < 1692 && // Vases
					(t.TileFrameY <= 54 || t.TileFrameY >= 162 && t.TileFrameY < 216))
				{
					t.TileFrameX += (short)(color == DungeonColor.Pink ? 72 : 36);
				}
				else if (t.TileType == TileID.GrandfatherClocks && t.TileFrameX >= 1080 && t.TileFrameX < 1116) // Clocks
				{
					t.TileFrameX += (short)(color == DungeonColor.Pink ? 72 : 36);
				}
				else if (t.TileType == TileID.Benches && t.TileFrameX >= 324 && t.TileFrameX < 378 && t.TileFrameY < 36) // Benches
				{
					t.TileFrameX += (short)(color == DungeonColor.Pink ? 108 : 54);
				}
				else if (t.TileType == TileID.Pianos && t.TileFrameX >= 594 && t.TileFrameX < 648 && t.TileFrameY < 36) // Pianos
				{
					t.TileFrameX += (short)(color == DungeonColor.Pink ? 108 : 54);
				}
				else if (t.TileType == TileID.Candles && t.TileFrameY >= 22 && t.TileFrameY < 44) // Candles
				{
					t.TileFrameY += (short)(color == DungeonColor.Pink ? 44 : 22);
				}
				else if (t.TileType == TileID.Lamps && t.TileFrameY >= 1296 && t.TileFrameY < 1350 && t.TileFrameX < 34) // Lamps
				{
					t.TileFrameY += (short)(color == DungeonColor.Pink ? 108 : 54);
				}
				else if (t.TileType == TileID.ClosedDoor && t.TileFrameY >= 864 && t.TileFrameY < 918 && t.TileFrameX < 72) // Door (C)
				{
					t.TileFrameY += (short)(color == DungeonColor.Pink ? 108 : 54);
				}
				else if (t.TileType == TileID.OpenDoor && t.TileFrameY >= 864 && t.TileFrameY < 918 && t.TileFrameX < 54) // Door (0)
				{
					t.TileFrameY += (short)(color == DungeonColor.Pink ? 108 : 54);
				}
				else if (t.TileType == TileID.Dressers && t.TileFrameX >= 270 && t.TileFrameX < 324 && t.TileFrameY < 36) // Dressers
				{
					t.TileFrameX += (short)(color == DungeonColor.Pink ? 108 : 54);
				}

				if (WallToAlt.TryGetValue(t.WallType, out int id))
				{
					t.WallType = (ushort)id;
				}
			}

			progress.Value = i / (Main.maxTilesX - 4f);
		}
	}
}