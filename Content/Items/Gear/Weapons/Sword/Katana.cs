namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class Katana : Sword
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Sword/Katana";
	
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