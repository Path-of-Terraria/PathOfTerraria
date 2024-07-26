using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.WarShields;

internal class StoneWarShield : WarShield
{
	public override int MinDropItemLevel => 5;
	public override ShieldData Data => new(18, 100, 12f, DustID.Stone);

	public override void Defaults()
	{
		base.Defaults();

		Item.damage = 13;
		Item.Size = new(20, 24);
	}
}
