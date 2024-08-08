using PathOfTerraria.Common.Systems;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Consumables.Maps;

internal class LowTierMap : Map
{
	public override int MaxUses => 1;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
	}

	public override void OpenMap()
	{
		MappingSystem.EnterMap(this);
	}

	public override string GenerateName(string defaultName)
	{
		return defaultName;
	}
}