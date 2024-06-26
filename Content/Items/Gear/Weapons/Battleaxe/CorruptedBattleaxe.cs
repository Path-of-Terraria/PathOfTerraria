namespace PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;

internal class CorruptedBattleaxe : IronBattleaxe
{
	public override float DropChance => 1f;
	
	public override void Defaults()
	{
		base.Defaults();
		Item.width = 52;
		Item.height = 52;
	}
}