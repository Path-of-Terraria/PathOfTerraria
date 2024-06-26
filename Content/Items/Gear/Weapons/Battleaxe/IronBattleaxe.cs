namespace PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;

internal class IronBattleaxe : Battleaxe
{
	public override float DropChance => 1f;
	
	public override void Defaults()
	{
		base.Defaults();
		Item.damage = 16;
	}
}