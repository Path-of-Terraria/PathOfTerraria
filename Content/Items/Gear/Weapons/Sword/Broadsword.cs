namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class Broadsword : Sword
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.width = 52; 
		Item.height = 52;
		Item.damage = 35;
		Item.useTime = 60;
		Item.useAnimation = 65;
	}
}
