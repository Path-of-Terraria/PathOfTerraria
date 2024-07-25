namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class Broadsword : Sword
{
	public override void Defaults()
	{
		base.Defaults();
		Item.width = 52; 
		Item.height = 52;
		Item.damage = 35;
		Item.useTime = 65; 
		Item.useAnimation = 65;
	}
}