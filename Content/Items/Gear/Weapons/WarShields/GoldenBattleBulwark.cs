using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.WarShields;

internal class GoldenBattleBulwark : WarShield
{
	public override int MinDropItemLevel => 20;
	public override ShieldData Data => new(13, 120, 13.5f, DustID.Gold);

	public override void Defaults()
	{
		base.Defaults();

		Item.damage = 26;
		Item.Size = new(26);
		Item.knockBack = 9;
	}
}
