using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.Systems;

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

		this.GetStaticData().DropChance = 1f;
	}

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