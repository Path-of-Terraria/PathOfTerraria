namespace PathOfTerraria.Content.Items.Gear.Weapons.Boomerangs;

internal class SilverBoomerang : Boomerang
{
	public override float DropChance => 1f;
	public override int ItemLevel => 14;

	public override void Defaults()
	{
		base.Defaults();
		Item.damage = 15;
		Item.shootSpeed = 12;
		Item.autoReuse = true;
	}
}