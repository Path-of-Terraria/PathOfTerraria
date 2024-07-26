using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Consumables.Maps;

internal class LowTierMap : Map
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		this.GetStaticData().DropChance = 1f;
	}
}