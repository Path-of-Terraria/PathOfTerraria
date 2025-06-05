using HousingAPI.Common;
using HousingAPI.Common.Helpers;
using Humanizer;
using PathOfTerraria.Common.NPCs;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.RoomTypes;

/// <summary> Allows <see cref="ITavernNPC"/>s to share a room. </summary>
internal class TavernDetours : ILoadable
{
	public void Load(Mod mod)
	{
		On_TownRoomManager.CanNPCsLiveWithEachOther_NPC_NPC += ForceShareRoom;
	}

	private static bool ForceShareRoom(On_TownRoomManager.orig_CanNPCsLiveWithEachOther_NPC_NPC orig, TownRoomManager self, NPC npc1, NPC npc2)
	{
		bool value = orig(self, npc1, npc2);
		if (npc1.ModNPC is ITavernNPC)
		{
			return true;
		}

		return value;
	}

	public void Unload() { }
}

/// <summary> Prevents tavern NPCs from moving into normal rooms. </summary>
public class TavernGlobal : GlobalRoomType
{
	public override bool? AllowNPC(int npcType)
	{
		return ContentSamples.NpcsByNetId[npcType].ModNPC is ITavernNPC ? false : null;
	}
}

public class Tavern : ModRoomType
{
	public override bool Priority => true;

	protected override bool RoomCheck(int x, int y, RoomScanner results)
	{
		bool value = base.RoomCheck(x, y, results);

		if (value && results.NumTiles < 100)
		{
			ErrorLog = Language.GetTextValue("TownNPCHousingFailureReasons.RoomIsTooSmall");
			value = false;
		}

		return value;
	}

	protected override bool RoomNeeds(RoomScanner results)
	{
		const string path = $"Mods.{PoTMod.ModName}.Rooms.";
		List<string> objects = [];

		if (CountNeed(TileID.Sets.RoomNeeds.CountsAsTable, results) < 3)
		{
			objects.Add(Language.GetTextValue(path + "Tavern.Error.Tables"));
		}

		if (!results.ContainsTile(TileID.Kegs))
		{
			objects.Add(Language.GetTextValue(path + "Tavern.Error.Keg"));
		}

		CountChairTypes(results, out int chairs, out int stools);
		if (chairs < 3)
		{
			objects.Add(Language.GetTextValue(path + "Tavern.Error.Chairs"));
		}

		if (stools < 3)
		{
			objects.Add(Language.GetTextValue(path + "Tavern.Error.BarStools"));
		}

		if (!results.ContainsTile(TileID.Signs))
		{
			objects.Add(Language.GetTextValue(path + "Tavern.Error.Sign"));
		}

		if (!results.HasRoomNeed(TileID.Sets.RoomNeeds.CountsAsTorch))
		{
			objects.Add(Language.GetTextValue("Game.HouseLightSource"));
		}

		if (!results.HasRoomNeed(TileID.Sets.RoomNeeds.CountsAsDoor))
		{
			objects.Add(Language.GetTextValue("Game.HouseDoor"));
		}

		if (objects.Count != 0)
		{
			if (objects.Count == 1)
			{
				ErrorLog = Language.GetTextValue(path + "Common.Missing1").FormatWith(objects[0]);
			}
			else if (objects.Count == 2)
			{
				ErrorLog = Language.GetTextValue(path + "Common.Missing2").FormatWith(objects[0], objects[1]);
			}
			else
			{
				string listText = Language.GetTextValue(path + "Common.MissingList");
				string init = string.Empty;

				for (int i = 0; i < objects.Count - 2; i++)
				{
					init += listText.FormatWith(objects[i]);
				}

				ErrorLog = Language.GetTextValue(path + "Common.Missing3").FormatWith(init, objects[^2], objects[^1]);
			}

			return false;
		}

		return true;
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

	/// <summary> Manually counts chair tiles using <see cref="RoomScanner.Scanned"/>. </summary>
	private static void CountChairTypes(RoomScanner results, out int chairs, out int barStools)
	{
		const int fullFrameHeight = 40;

		int chairCount = 0;
		int stoolCount = 0;

		foreach (Point16 coord in results.Scanned)
		{
			Tile t = Main.tile[coord.X, coord.Y];
			if (t.TileType == TileID.Chairs && TileObjectData.IsTopLeft(t))
			{
				if (t.TileFrameY == fullFrameHeight * 21) //Bar stool
				{
					stoolCount++;
				}
				else
				{
					chairCount++;
				}
			}
			else if (TileID.Sets.RoomNeeds.CountsAsChair.Contains(t.TileType) && TileObjectData.IsTopLeft(t))
			{
				chairCount++;
			}
		}

		chairs = chairCount;
		barStools = stoolCount;
	}
}