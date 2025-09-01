﻿using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.WarShields;

internal class PlatinumWarShield : WarShield
{
	public override ShieldData Data => new(20, 120, 14, DustID.Platinum);

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.MinDropItemLevel = 20;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 22;
		Item.Size = new(26);
		Item.knockBack = 9;
		Item.value = Item.buyPrice(0, 0, 1, 30);
	}
}
