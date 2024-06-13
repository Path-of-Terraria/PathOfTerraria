namespace PathOfTerraria.Content.Items.Gear.Weapons.Boomerangs;

internal class WoodenBoomerang : Boomerang
{
	public override float DropChance => 1f;

	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.damage = 5;
		Item.shootSpeed = 12;
		Item.autoReuse = true;
	}
}