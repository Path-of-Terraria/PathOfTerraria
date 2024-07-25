namespace PathOfTerraria.Content.Items.Gear.Weapons.WarShields;

internal class WoodPlank : WarShield
{
	public override void Defaults()
	{
		base.Defaults();

		Item.damage = 7;
		Item.Size = new(22, 26);
	}
}
