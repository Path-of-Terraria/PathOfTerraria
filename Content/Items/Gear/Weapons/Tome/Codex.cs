namespace PathOfTerraria.Content.Items.Gear.Weapons.Tome;

internal class Codex : Spellbook
{
	public override int MinDropItemLevel => 60;
	
	public override void Defaults()
	{
		base.Defaults();
		Item.damage = 50;
	}
}