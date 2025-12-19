namespace PathOfTerraria.Content.Items.Gear.Offhands.Shields;

internal class LeatherBuckler : Shield
{
	protected override float BlockChance => 0.08f;
	protected override float SpeedReduction => 1.2f;

	protected override void InternalDefaults()
	{
		Item.Size = new(26);
		Item.value = Item.buyPrice(0, 0, 0, 10);
		Item.defense = 1;
	}
}
