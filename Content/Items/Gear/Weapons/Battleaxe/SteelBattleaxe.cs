namespace PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;

internal class SteelBattleaxe : Battleaxe
{
	public override float DropChance => 1f;
	public override int MinDropItemLevel => 18;
	
	public override void Defaults()
	{
		base.Defaults();
		Item.damage = 26;
		Item.width = 40;
		Item.height = 40;
	}
}