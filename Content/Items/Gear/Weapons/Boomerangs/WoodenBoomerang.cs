﻿using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Boomerangs;

internal class WoodenBoomerang : Boomerang
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;

		GearAlternatives.Register(Type, ItemID.WoodenBoomerang);
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 6;
		Item.shootSpeed = 12;
		Item.autoReuse = true;
		Item.value = Item.buyPrice(0, 0, 0, 10);
	}
}