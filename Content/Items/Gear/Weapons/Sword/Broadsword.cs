namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal abstract class Broadsword : Sword
{
	protected override string GearLocalizationCategory => "Broadsword";
	
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.width = 50; 
		Item.height = 50;
		Item.damage = 15;
		Item.useTime = 60;
		Item.useAnimation = 65;
		Item.useTurn = true;
		Item.value = Item.buyPrice(0, 0, 1, 0);
	}
}
