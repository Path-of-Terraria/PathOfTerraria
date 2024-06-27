namespace PathOfTerraria.Content.Items.Gear.Weapons.Boomerangs;

internal class IronBoomerang : Boomerang
{
	public override float DropChance => 1f;
	public override int MinDropItemLevel => 5;

	public override void Defaults()
	{
		base.Defaults();
		Item.damage = 9;
		Item.shootSpeed = 12;
		Item.autoReuse = true;
	}
}