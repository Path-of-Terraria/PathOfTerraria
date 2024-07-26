using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.WarShields;

internal class PlatinumWarShield : WarShield
{
	public override int MinDropItemLevel => 20;
	public override ShieldData Data => new(20, 120, 14, DustID.Platinum);

	public override void Defaults()
	{
		base.Defaults();

		Item.damage = 22;
		Item.Size = new(26);
		Item.knockBack = 9;
	}
}
