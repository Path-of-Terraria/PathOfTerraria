using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.WarShields;

internal class SteelWarShield : WarShield
{
	public override int MinDropItemLevel => 25;
	public override ShieldData Data => new(16, 110, 15, DustID.t_SteampunkMetal);

	public override void Defaults()
	{
		base.Defaults();

		Item.damage = 30;
		Item.Size = new(26);
		Item.knockBack = 9;
	}
}
