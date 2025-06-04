using HousingAPI.Common;
using HousingAPI.Common.Helpers;
using Terraria.ID;

namespace PathOfTerraria.Content.RoomTypes;

public class Tavern : ModRoomType
{
	public override bool Priority => true;

	protected override bool RoomCheck(int x, int y, RoomScanner results)
	{
		bool value = base.RoomCheck(x, y, results);

		if (value && results.NumTiles < 100)
		{
			ErrorLog += "(DEBUG) room size";
			value = false;
		}

		return value;
	}

	protected override bool RoomNeeds(RoomScanner results)
	{
		bool failed = false;

		if (CountNeed(TileID.Sets.RoomNeeds.CountsAsTable, results) < 3)
		{
			ErrorLog += "(DEBUG) tables";
			failed = true;
		}

		if (!results.ContainsTile(TileID.Kegs))
		{
			ErrorLog += "(DEBUG) keg";
			failed = true;
		}

		if (CountNeed(TileID.Sets.RoomNeeds.CountsAsChair, results) < 3)
		{
			ErrorLog += "(DEBUG) chairs";
			failed = true;
		}

		if (!results.ContainsTile(TileID.Signs))
		{
			ErrorLog += "(DEBUG) signs";
			failed = true;
		}

		if (!results.HasRoomNeed(TileID.Sets.RoomNeeds.CountsAsTorch))
		{
			ErrorLog += "(DEBUG) light source";
			failed = true;
		}

		if (!results.HasRoomNeed(TileID.Sets.RoomNeeds.CountsAsDoor))
		{
			ErrorLog += "(DEBUG) door";
			failed = true;
		}

		return !failed;
	}

	private static int CountNeed(int[] set, RoomScanner results)
	{
		int count = 0;
		foreach (int type in set)
		{
			count += results.TileCount(type);
		}

		return count;
	}

	protected override bool AllowNPC(int npcType)
	{
		return false;
	}
}