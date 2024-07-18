using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Javelins;

internal class LeadDangpa : Javelin
{
	public override Vector2 ItemSize => new(70);
	public override int DeathDustType => DustID.Lead;
	public override int MinDropItemLevel => 12;

	public override void Defaults()
	{
		base.Defaults();

		Item.damage = 10;
	}
}
