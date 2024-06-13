namespace PathOfTerraria.Content.Items.Gear.Weapons.Boomerangs;

internal class StoneBoomerang : Boomerang
{
	public override float DropChance => 1f;

	public override void Defaults()
	{
		base.Defaults();
		Item.damage = 7;
		Item.shootSpeed = 11;
		Item.autoReuse = true;
	}
}