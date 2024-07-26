using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Battleaxe;

internal class IronBattleaxe : Battleaxe
{

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 12;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 16;
		Item.Size = new Vector2(34);
	}
}