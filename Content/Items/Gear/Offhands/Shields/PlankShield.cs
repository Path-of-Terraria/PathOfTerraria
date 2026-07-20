namespace PathOfTerraria.Content.Items.Gear.Offhands.Shields;

internal class PlankShield : Shield
{
	internal override float BaseBlockChance => 0.22f;
	protected override float SpeedReduction => 5f;

	protected override void InternalDefaults()
	{
		Item.Size = new(18, 24);
		Item.value = Item.buyPrice(0, 0, 0, 10);
		Item.defense = 2;
	}
}
