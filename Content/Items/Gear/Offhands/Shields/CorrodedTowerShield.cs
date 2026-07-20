using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Offhands.Shields;

internal class CorrodedTowerShield : Shield
{
	internal override float BaseBlockChance => 0.22f;
	protected override float ImplicitBlockChance => 0.05f;
	protected override float SpeedReduction => 1.5f;

	protected override void InternalDefaults()
	{
		Item.Size = new(18, 30);
		Item.value = Item.buyPrice(0, 0, 0, 10);
		Item.defense = 3;
	}
}
