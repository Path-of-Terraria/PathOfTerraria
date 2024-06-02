namespace PathOfTerraria.Content.Items.Gear.Weapons.Tome;

internal class Codex : Spellbook
{
	public Codex()
	{
		ItemLevel = 60;
	}
	
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.damage = 50;
	}
}