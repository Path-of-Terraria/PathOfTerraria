using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Boomerangs;

internal class SilverBoomerang : Boomerang
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 14;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 15;
		Item.shootSpeed = 12;
		Item.autoReuse = true;
	}
}