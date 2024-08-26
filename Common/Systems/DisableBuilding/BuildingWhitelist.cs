using PathOfTerraria.Common.Subworlds;
using SubworldLibrary;
using System.Collections.Generic;

namespace PathOfTerraria.Common.Systems.DisableBuilding;

internal class BuildingWhitelist
{
	public static HashSet<int> GetMiningWhitelist()
	{
		HashSet<int> whitelist = [];

		if (SubworldSystem.Current is BossDomainSubworld domain)
		{
			int[] tiles = domain.WhitelistedMiningTiles;

			foreach (int value in tiles)
			{
				whitelist.Add(value);
			}
		}

		return whitelist;
	}

	public static bool InMiningWhitelist(int tileType)
	{
		return GetMiningWhitelist().Contains(tileType);	
	}

	public static HashSet<int> GetCuttingWhitelist()
	{
		HashSet<int> whitelist = [];

		if (SubworldSystem.Current is BossDomainSubworld domain)
		{
			int[] tiles = domain.WhitelistedCutTiles;

			foreach (int value in tiles)
			{
				whitelist.Add(value);
			}
		}

		return whitelist;
	}

	public static bool InCuttingWhitelist(int tileType)
	{
		return GetCuttingWhitelist().Contains(tileType);
	}
}
