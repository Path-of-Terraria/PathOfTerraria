using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Armor.Shields;

internal class CrimsonBulwark : Shield
{
	protected override float BlockChance => 0.23f;
	protected override float SpeedReduction => 0.05f;

	public override void SetStaticDefaults()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 18;
	}

	protected override void InternalDefaults()
	{
		Item.Size = new(28, 30);
		Item.value = Item.buyPrice(0, 0, 3, 0);
		Item.defense = 8;
	}
}
