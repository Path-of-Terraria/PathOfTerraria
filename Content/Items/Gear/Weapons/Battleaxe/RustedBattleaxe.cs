namespace PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;

internal class RustedBattleaxe : Battleaxe
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Battleaxe/{GetType().Name}";
	public override float DropChance => 1f;
}