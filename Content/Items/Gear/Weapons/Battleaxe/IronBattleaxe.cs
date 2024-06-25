namespace PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;

internal class IronBattleaxe : Battleaxe
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Battleaxe/{GetType().Name}";
	public override float DropChance => 1f;
	
	public override void Defaults()
	{
		base.Defaults();
		Item.damage = 16;
	}
}