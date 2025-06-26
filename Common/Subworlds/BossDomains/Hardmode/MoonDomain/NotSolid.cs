using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain;

public class NotSolid : GenCondition
{
	protected override bool CheckValidity(int x, int y)
	{
		return !_tiles[x, y].HasTile || !Main.tileSolid[_tiles[x, y].TileType];
	}
}