namespace PathOfTerraria.Content.Items.Gear.Weapons.Tome;

internal class Codex : Spellbook
{
	public override int ItemLevel => 60;
	
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.damage = 50;
	}
}