﻿using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class SteelBroadsword : Broadsword
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 20;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 38;
		Item.width = 46;
		Item.height = 46;
		Item.UseSound = SoundID.Item1;
		Item.value = Item.buyPrice(0, 0, 2, 50);

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = ItemType.Sword;
	}
}