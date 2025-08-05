using PathOfTerraria.Common.Subworlds;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.DisableBuilding;

internal class BuildingWhitelist
{
	public readonly struct Whitelist(HashSet<int> allowed, List<FramedTileBlockers> framedBlockers)
	{
		public readonly HashSet<int> Allowed = allowed;
		public readonly List<FramedTileBlockers> FramedBlockers = framedBlockers;

		public void Add(int id)
		{
			Allowed.Add(id);
		}

		public bool Contains(int id)
		{
			return Allowed.Contains(id);
		}

		public bool IsAFramedExclusion(int id, Point16? frame)
		{
			if (frame is null)
			{
				return false;
			}

			foreach (FramedTileBlockers tile in FramedBlockers)
			{
				if (tile.TileType == id && (tile.Frame.X is null || tile.Frame.X == frame.Value.X) && (tile.Frame.Y is null || tile.Frame.Y == frame.Value.Y))
				{
					return true;
				}
			}

			return false;
		}
	}

	public enum WhitelistUse : byte
	{
		Mining,
		Cutting,
		Placing,
		Exploding,
	}

	public static HashSet<int> DefaultWhitelist = [TileID.Torches, TileID.Rope, TileID.Tombstones];

	public static Whitelist GetUsedWhitelist(WhitelistUse use)
	{
		HashSet<int> results = use is WhitelistUse.Placing or WhitelistUse.Mining ? DefaultWhitelist : [];
		List<FramedTileBlockers> framed = [];

		if (SubworldSystem.Current is MappingWorld domain)
		{
			domain.ModifyDefaultWhitelist(results, use, framed);
		}

		return new(results, framed);
	}

	public static Whitelist GetMiningWhitelist()
	{
		Whitelist whitelist = GetUsedWhitelist(WhitelistUse.Mining);

		if (SubworldSystem.Current is MappingWorld domain)
		{
			int[] tiles = domain.WhitelistedMiningTiles;

			foreach (int value in tiles)
			{
				whitelist.Add(value);
			}
		}

		return whitelist;
	}

	public static bool InMiningWhitelist(int tileType, Point16? tileFrame)
	{
		Whitelist whitelist = GetMiningWhitelist();
		return whitelist.Contains(tileType) && !whitelist.IsAFramedExclusion(tileType, tileFrame);	
	}

	public static Whitelist GetCuttingWhitelist()
	{
		Whitelist whitelist = GetUsedWhitelist(WhitelistUse.Cutting);

		if (SubworldSystem.Current is MappingWorld domain)
		{
			int[] tiles = domain.WhitelistedCutTiles;

			foreach (int value in tiles)
			{
				whitelist.Add(value);
			}
		}

		return whitelist;
	}

	public static bool InCuttingWhitelist(int tileType, Point16? tileFrame)
	{
		Whitelist whitelist = GetCuttingWhitelist();
		return whitelist.Contains(tileType) && !whitelist.IsAFramedExclusion(tileType, tileFrame);
	}

	public static Whitelist GetPlacingWhitelist()
	{
		Whitelist whitelist = GetUsedWhitelist(WhitelistUse.Placing);

		if (SubworldSystem.Current is MappingWorld domain)
		{
			int[] tiles = domain.WhitelistedPlaceableTiles;

			foreach (int value in tiles)
			{
				whitelist.Add(value);
			}
		}

		return whitelist;
	}

	internal static bool InPlacingWhitelist(int tileType, Point16? tileFrame)
	{
		Whitelist whitelist = GetPlacingWhitelist();
		return whitelist.Contains(tileType) && !whitelist.IsAFramedExclusion(tileType, tileFrame);
	}

	public static Whitelist GetExplodingWhitelist()
	{
		Whitelist whitelist = GetUsedWhitelist(WhitelistUse.Exploding);

		if (SubworldSystem.Current is MappingWorld domain)
		{
			int[] tiles = domain.WhitelistedExplodableTiles;

			foreach (int value in tiles)
			{
				whitelist.Add(value);
			}
		}

		return whitelist;
	}

	internal static bool InExplodingWhitelist(int tileType, Point16? tileFrame)
	{
		Whitelist whitelist = GetExplodingWhitelist();
		return whitelist.Contains(tileType) && !whitelist.IsAFramedExclusion(tileType, tileFrame);
	}
}
