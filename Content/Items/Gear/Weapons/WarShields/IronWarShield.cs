using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.WarShields;

internal class IronWarShield : WarShield
{
	public override int MinDropItemLevel => 12;
	public override ShieldData Data => new(20, 130, 12, DustID.Iron);

	public override void Defaults()
	{
		base.Defaults();

		Item.damage = 17;
		Item.Size = new(28);
	}
}
