using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Offhands.Talismans;

internal class WoodenTalisman : Talisman
{
	protected override float MinionDamage => 0.03f;

	public override void SetStaticDefaults()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 1;
	}

	protected override void InternalDefaults()
	{
		Item.Size = new(28, 36);
		Item.value = Item.buyPrice(0, 0, 1, 0);
		Item.defense = 2;
	}
}