
namespace PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;

internal class RustedBattleaxe : Battleaxe
{
	public override float DropChance => 1f;

	public override void Defaults()
	{
		base.Defaults();

		Item.width = 38;
		Item.height = 38;
	}

	public override void UseStyle(Player player, Rectangle heldItemFrame)
	{
		
	}
}