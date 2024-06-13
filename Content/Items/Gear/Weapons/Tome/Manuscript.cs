namespace PathOfTerraria.Content.Items.Gear.Weapons.Tome;

internal class Manuscript : Spellbook
{
	public override int ItemLevel => 20;
	
	public override void Defaults()
	{
		base.Defaults();
		Item.damage = 20;
	}
}