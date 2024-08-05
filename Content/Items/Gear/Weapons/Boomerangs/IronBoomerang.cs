using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Boomerangs;

internal class IronBoomerang : Boomerang
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 5;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 9;
		Item.shootSpeed = 12;
		Item.autoReuse = true;
	}
}