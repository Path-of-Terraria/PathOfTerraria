using Terraria.DataStructures;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain;

public class SetTileNoClear(ushort type, bool setSelfFrames = false, bool setNeighborFrames = true) : GenAction
{
	private readonly ushort _type = type;
	private readonly bool _doFraming = setSelfFrames;
	private readonly bool _doNeighborFraming = setNeighborFrames;

	public override bool Apply(Point origin, int x, int y, params object[] args)
	{
		Tile tile = _tiles[x, y];
		tile.TileType = _type;

		if (_doFraming)
		{
			WorldUtils.TileFrame(x, y, _doNeighborFraming);
		}

		return UnitApply(origin, x, y, args);
	}
}
