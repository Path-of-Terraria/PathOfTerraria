using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;

internal class SteelBattleaxe : Battleaxe
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 18;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 26;
		Item.width = 40;
		Item.height = 40;
	}
}