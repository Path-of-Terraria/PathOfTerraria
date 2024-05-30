using PathOfTerraria.Core.Systems;
using PathOfTerraria.Core.Systems.Affixes;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace PathOfTerraria.Content.Items.Consumables.Maps;

public class CaveMap : Map
{
	public int RoomSizeMin = 20;
	public int RoomSizeMax = 44;
	public int BossRoomSize = 50;
	public int SpawnRoomSize = 16;
	public int ExtraRoomDist = 20;

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