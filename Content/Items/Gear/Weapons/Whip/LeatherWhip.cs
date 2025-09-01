﻿using PathOfTerraria.Content.Projectiles.Whip;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Whip;

internal class LeatherWhip : Whip
{
	public override WhipDrawData DrawData => new(new(10, 26), new(0, 36, 10, 16), new(0, 64, 10, 16), new(0, 92, 10, 16), new(0, 120, 10, 18), false);
	public override WhipSettings WhipSettings => ContentSamples.ProjectilesByType[ProjectileID.BlandWhip].WhipSettings;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.MinDropItemLevel = 8;
	}
	
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.DefaultToWhip(ModContent.ProjectileType<WhipBaseProjectile>(), 12, 2, 4);
		Item.channel = true;
		Item.value = Item.buyPrice(0, 0, 0, 20);
	}
}