namespace PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;

internal class IronBattleaxe : Battleaxe
{
	public override float DropChance => 1f;
	public override int MinDropItemLevel => 12;
	
	public override void Defaults()
	{
		base.Defaults();
		Item.damage = 16;
		Item.Size = new Vector2(34);
	}
}