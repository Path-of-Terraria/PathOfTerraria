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
}
