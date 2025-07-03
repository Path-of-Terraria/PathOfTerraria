using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain;

public class RemoveTileAction : GenAction
{
	public override bool Apply(Point origin, int x, int y, params object[] args)
	{
		Tile tile = Main.tile[x, y];
		tile.HasTile = false;
		return UnitApply(origin, x, y, args);
	}
}