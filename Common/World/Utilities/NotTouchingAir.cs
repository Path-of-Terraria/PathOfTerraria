using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.World.Utilities;

public class NotTouchingAir(bool useDiagonals = false) : GenAction
{
	private static readonly int[] DIRECTIONS = [0, -1, 1, 0, -1, 0, 0, 1, -1, -1, 1, -1, -1, 1, 1, 1];
	
	private readonly bool _useDiagonals = useDiagonals;

	public override bool Apply(Point origin, int x, int y, params object[] args)
	{
		int num = (_useDiagonals ? 16 : 8);

		for (int i = 0; i < num; i += 2)
		{
			if (_tiles[x + DIRECTIONS[i], y + DIRECTIONS[i + 1]].HasTile)
			{
				return Fail();
			}
		}

		return UnitApply(origin, x, y, args);
	}
}
