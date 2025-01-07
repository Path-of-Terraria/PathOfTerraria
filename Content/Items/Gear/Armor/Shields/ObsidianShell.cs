using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Armor.Shields;

internal class ObsidianShell : Shield
{
	protected override float BlockChance => 0.21f;
	protected override float SpeedReduction => 0.04f;

	public override void SetStaticDefaults()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 13;
	}

	protected override void InternalDefaults()
	{
		Item.Size = new(26, 30);
		Item.value = Item.buyPrice(0, 0, 3, 0);
		Item.defense = 5;
	}
}
