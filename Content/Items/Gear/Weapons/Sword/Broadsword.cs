namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class Broadsword : Sword
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Sword/Broadsword";
	
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.width = 52; 
		Item.height = 52;
		Item.damage = 35;
		Item.useTime = 65; 
		Item.useAnimation = 65;
	}
}