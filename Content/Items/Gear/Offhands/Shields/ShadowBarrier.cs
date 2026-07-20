using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Offhands.Shields;

internal class ShadowBarrier : Shield
{
	internal override float BaseBlockChance => 0.23f;
	protected override float ImplicitBlockChance => 0.15f;
	protected override float SpeedReduction => 1.2f;

	public override void SetStaticDefaults()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 18;
	}

	protected override void InternalDefaults()
	{
		Item.Size = new(26, 32);
		Item.value = Item.buyPrice(0, 0, 10, 0);
		Item.defense = 5;
	}
}
