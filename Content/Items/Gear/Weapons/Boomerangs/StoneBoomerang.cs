﻿using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Boomerangs;

internal class StoneBoomerang : Boomerang
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 8;
		Item.shootSpeed = 11;
		Item.autoReuse = true;
		Item.value = Item.buyPrice(0, 0, 0, 60);
	}
}