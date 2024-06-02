namespace PathOfTerraria.Content.Items.Gear.Weapons.Tome;

internal class Grimoire : Spellbook
{
	public override int ItemLevel => 80;
	
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.damage = 75;
	}
}