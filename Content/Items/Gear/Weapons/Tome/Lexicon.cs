namespace PathOfTerraria.Content.Items.Gear.Weapons.Tome;

internal class Lexicon : Spellbook
{
	public override int ItemLevel => 40;
	
	public override void Defaults()
	{
		base.Defaults();
		Item.damage = 35;
	}
}