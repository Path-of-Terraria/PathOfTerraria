using PathOfTerraria.Common.Subworlds;
using SubworldLibrary;

namespace PathOfTerraria.Common.Systems.DisableBuilding;

internal class StopExplodingTile : GlobalTile
{
	public override bool CanExplode(int i, int j, int type)
	{
		return SubworldSystem.Current is not BossDomainSubworld || BuildingWhitelist.InMiningWhitelist(type);
	}
}
