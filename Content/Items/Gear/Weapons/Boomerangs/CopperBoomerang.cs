﻿namespace PathOfTerraria.Content.Items.Gear.Weapons.Boomerangs;

internal class CopperBoomerang : Boomerang
{
	public override float DropChance => 1f;
	public override int MinDropItemLevel => 8;

	public override void Defaults()
	{
		base.Defaults();
		Item.damage = 12;
		Item.shootSpeed = 12;
		Item.autoReuse = true;
	}
}