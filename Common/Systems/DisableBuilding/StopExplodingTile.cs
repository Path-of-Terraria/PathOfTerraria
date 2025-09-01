using PathOfTerraria.Common.Subworlds;
using SubworldLibrary;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Systems.DisableBuilding;

internal class StopExplodingTile : GlobalTile
{
	public override bool CanReplace(int i, int j, int type, int tileTypeBeingPlaced)
	{
		return SubworldSystem.Current is not MappingWorld;
	}

	public override bool CanExplode(int i, int j, int type)
	{
		Tile tile = Main.tile[i, j];
		Point16 frame = new(tile.TileFrameX, tile.TileFrameY);
		return SubworldSystem.Current is not MappingWorld || BuildingWhitelist.InMiningWhitelist(type, frame) || BuildingWhitelist.InExplodingWhitelist(type, frame);
	}

	public override bool Slope(int i, int j, int type)
	{
		return SubworldSystem.Current is not MappingWorld;
	}
}
