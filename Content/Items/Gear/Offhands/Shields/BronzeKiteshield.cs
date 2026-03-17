using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Offhands.Shields;

internal class BronzeKiteshield : Shield
{
	protected override float BlockChance => 0.1f;
	protected override float SpeedReduction => 1.4f;

	public override void SetStaticDefaults()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 8;
	}

	protected override void InternalDefaults()
	{
		Item.Size = new(24, 30);
		Item.value = Item.buyPrice(0, 0, 1, 0);
		Item.defense = 3;
	}
}
