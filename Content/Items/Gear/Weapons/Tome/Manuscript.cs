namespace PathOfTerraria.Content.Items.Gear.Weapons.Tome;

internal class Manuscript : Spellbook
{
	public Manuscript()
	{
		ItemLevel = 20;
	}
	
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.damage = 20;
	}
}