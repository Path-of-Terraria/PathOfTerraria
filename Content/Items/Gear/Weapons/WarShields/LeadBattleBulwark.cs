using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.WarShields;

internal class LeadBattleBulwark : WarShield
{
	public override int MinDropItemLevel => 12;
	public override ShieldData Data => new(15, 120, 12, DustID.Lead);

	public override void Defaults()
	{
		base.Defaults();

		Item.damage = 14;
		Item.Size = new(28);
	}
}
