namespace PathOfTerraria.Content.Items.Gear.Weapons.WarShields;

internal class WoodPlank : WarShield
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 7;
		Item.Size = new(22, 26);
		Item.value = Item.buyPrice(0, 0, 0, 10);
	}
}
