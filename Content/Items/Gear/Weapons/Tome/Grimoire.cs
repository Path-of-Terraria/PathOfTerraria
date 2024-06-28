namespace PathOfTerraria.Content.Items.Gear.Weapons.Tome;

internal class Grimoire : Spellbook
{
	public override int MinDropItemLevel => 80;
	
	public override void Defaults()
	{
		base.Defaults();
		Item.damage = 75;
	}
}