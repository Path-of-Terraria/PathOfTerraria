using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.World.Passes;
using PathOfTerraria.Core.Items;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Items.Consumables.Maps;

internal class CaveMap : Map
{
	public int RoomSizeMin = 14;
	public int RoomSizeMax = 22;
	public int BossRoomSize = 30;
	public int SpawnRoomSize = 10;
	public int ExtraRoomDist = 10;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
	}

	public override bool? UseItem(Player player)
	{
		Point16 pos = Main.MouseWorld.ToTileCoordinates16();
		bool done = RavencrestEntrancePass.FindPlacement(pos.X, pos.Y, out pos);
		//MappingSystem.EnterCaveMap(this);
		return true;
	}

	public override string GenerateName(string defaultName)
	{
		return "Test Cave";
	}
}