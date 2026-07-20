using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Offhands.Shields;

internal class ObsidianShell : Shield
{
	internal override float BaseBlockChance => 0.21f;
	protected override float SpeedReduction => 4f;

	public override void SetStaticDefaults()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 13;
	}

	protected override void InternalDefaults()
	{
		Item.Size = new(26, 30);
		Item.value = Item.buyPrice(0, 0, 6, 0);
		Item.defense = 5;
	}
}
