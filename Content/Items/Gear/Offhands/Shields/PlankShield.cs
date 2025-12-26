namespace PathOfTerraria.Content.Items.Gear.Offhands.Shields;

internal class PlankShield : Shield
{
	protected override float BlockChance => 0.02f;
	protected override float SpeedReduction => 1.5f;

	protected override void InternalDefaults()
	{
		Item.Size = new(18, 24);
		Item.value = Item.buyPrice(0, 0, 0, 10);
		Item.defense = 2;
	}
}
