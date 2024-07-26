namespace PathOfTerraria.Content.Items.Gear.Weapons.Tome;

internal class Manuscript : Spellbook
{
	public override int MinDropItemLevel => 20;
	
	public override void SetDefaults()
	{
		Item.damage = 20;
	}
}