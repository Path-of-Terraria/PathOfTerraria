using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Javelins;

internal class PlatinumGlaive : Javelin
{
	public override Vector2 ItemSize => new(98);
	public override int DeathDustType => DustID.Platinum;
	public override int MinDropItemLevel => 21;

	public override void Defaults()
	{
		base.Defaults();

		Item.damage = 16;
	}
}
