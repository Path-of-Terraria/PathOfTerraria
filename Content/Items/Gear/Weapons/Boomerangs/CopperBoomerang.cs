namespace PathOfTerraria.Content.Items.Gear.Weapons.Boomerangs;

internal class CopperBoomerang : Boomerang
{
	public override float DropChance => 1f;
	public override int ItemLevel => 8;

	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.damage = 12;
		Item.shootSpeed = 12;
		Item.autoReuse = true;
	}
}