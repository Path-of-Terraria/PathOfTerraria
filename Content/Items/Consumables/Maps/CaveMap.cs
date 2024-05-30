using PathOfTerraria.Core.Systems;
using PathOfTerraria.Core.Systems.Affixes;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Items.Consumables.Maps;

public class CaveMap : Map
{
	public int RoomSizeMin = 14;
	public int RoomSizeMax = 22;
	public int BossRoomSize = 30;
	public int SpawnRoomSize = 10;
	public int ExtraRoomDist = 10;

	public override bool? UseItem(Player player)
	{
		MappingSystem.EnterCaveMap(this);
		return true;
	}

	public override string GenerateName()
	{
		return "Test Cave";
	}
}