using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Offhands.Shields;

internal class CrimsonBulwark : Shield
{
	internal override float BaseBlockChance => 0.23f;
	protected override float ImplicitBlockChance => 0.18f;
	protected override float SpeedReduction => 1.5f;

	public override void SetStaticDefaults()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 25;
	}

	protected override void InternalDefaults()
	{
		Item.Size = new(28, 30);
		Item.value = Item.buyPrice(0, 0, 15, 0);
		Item.defense = 8;
	}
}
