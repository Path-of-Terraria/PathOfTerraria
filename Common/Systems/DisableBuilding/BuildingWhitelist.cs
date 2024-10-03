using PathOfTerraria.Common.Subworlds;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.DisableBuilding;

internal class BuildingWhitelist
{
	public static HashSet<int> DefaultWhitelist = [TileID.Torches, TileID.Rope];

	public static HashSet<int> GetMiningWhitelist()
	{
		HashSet<int> whitelist = new(DefaultWhitelist);

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

	public static HashSet<int> GetPlacingWhitelist()
	{
		HashSet<int> whitelist = [];

		if (SubworldSystem.Current is BossDomainSubworld domain)
		{
			int[] tiles = domain.WhitelistedPlaceableTiles;

			foreach (int value in tiles)
			{
				whitelist.Add(value);
			}
		}

		return whitelist;
	}

	internal static bool InPlacingWhitelist(int tileType)
	{
		return GetPlacingWhitelist().Contains(tileType);
	}
}
