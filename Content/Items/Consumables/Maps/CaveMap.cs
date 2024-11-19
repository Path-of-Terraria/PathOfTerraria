using PathOfTerraria.Common.Systems;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Consumables.Maps;

internal class CaveMap : Map
{
	public int RoomSizeMin = 14;
	public int RoomSizeMax = 22;
	public int BossRoomSize = 30;
	public int SpawnRoomSize = 10;
	public int ExtraRoomDist = 10;

	public override int MaxUses => 1;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
	}

	public override void OpenMap()
	{
		MappingSystem.EnterCaveMap(this);
	}

	public override string GenerateName(string defaultName)
	{
		return "Test Cave";
	}
}