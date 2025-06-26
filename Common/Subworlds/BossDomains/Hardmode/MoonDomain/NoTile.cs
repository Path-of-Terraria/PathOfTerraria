using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain;

public class NoTile : GenCondition
{
	protected override bool CheckValidity(int x, int y)
	{
		return !_tiles[x, y].HasTile;
	}
}