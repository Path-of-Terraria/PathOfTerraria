namespace PathOfTerraria.Content.Items.Gear.Armor.Shields;

internal class LeatherBuckler : Shield
{
	protected override float BlockChance => 0.18f;
	protected override float SpeedReduction => 0.02f;

	protected override void InternalDefaults()
	{
		Item.Size = new(26);
		Item.value = Item.buyPrice(0, 0, 3, 0);
		Item.defense = 1;
	}
}
