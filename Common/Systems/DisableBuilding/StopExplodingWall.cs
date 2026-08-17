using PathOfTerraria.Common.Subworlds;
using SubworldLibraryCommunityFork;

namespace PathOfTerraria.Common.Systems.DisableBuilding;

internal class StopExplodingWall : GlobalWall
{
	public override bool CanExplode(int i, int j, int type)
	{
		return SubworldSystem.Current is not MappingWorld;
	}
}
