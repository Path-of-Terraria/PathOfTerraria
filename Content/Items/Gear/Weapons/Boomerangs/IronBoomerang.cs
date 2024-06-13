namespace PathOfTerraria.Content.Items.Gear.Weapons.Boomerangs;

internal class IronBoomerang : Boomerang
{
	public override float DropChance => 1f;
	public override int ItemLevel => 5;

	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.damage = 9;
		Item.shootSpeed = 12;
		Item.autoReuse = true;
	}
}