namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class Katana : Sword
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		GearAlternatives.Register(Type, Terraria.ID.ItemID.Katana);
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 20;
		Item.useTime = 35; 
		Item.useAnimation = 35;
		Item.width = 48; 
		Item.height = 54;
	}
}