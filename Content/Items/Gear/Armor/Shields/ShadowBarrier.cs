using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Armor.Shields;

internal class ShadowBarrier : Shield
{
	protected override float BlockChance => 0.23f;
	protected override float SpeedReduction => 0.03f;

	public override void SetStaticDefaults()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 18;
	}

	protected override void InternalDefaults()
	{
		Item.Size = new(26, 32);
		Item.value = Item.buyPrice(0, 0, 3, 0);
		Item.defense = 5;
	}
}
