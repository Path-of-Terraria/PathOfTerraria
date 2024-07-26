using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Consumables.Maps;

internal class LowTierMap : Map
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
	}

	public override string GenerateName(Item item)
	{
		return IGenerateNameItem.GetDefaultName(item);
	}
}