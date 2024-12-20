namespace PathOfTerraria.Content.Items.Gear.Weapons.Shields;

internal class PlankShield : Shield
{
	public override void SetDefaults()
	{
		Item.Size = new(18, 24);
		Item.value = Item.buyPrice(0, 0, 3, 0);
	}
}
