using PathOfTerraria.Common.Subworlds;
using SubworldLibrary;
using System.Collections.Generic;

namespace PathOfTerraria.Common.Systems.DisableBuilding;

internal class BuildingWhitelist
{
	public static HashSet<int> GetWhitelist()
	{
		HashSet<int> whitelist = [];

		if (SubworldSystem.Current is BossDomainSubworld domain)
		{
			int[] tiles = domain.WhitelistedTiles;

			foreach (int value in tiles)
			{
				whitelist.Add(value);
			}
		}

		return whitelist;
	}

	public static bool InWhitelist(int tileType)
	{
		return GetWhitelist().Contains(tileType);	
	}
}
