
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;

internal class RustedBattleaxe : Battleaxe
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.width = 38;
		Item.height = 38;
	}

	public override void UseStyle(Player player, Rectangle heldItemFrame)
	{
		
	}
}